using System.Reflection;
using RPGCreator.Core.Parser.Graph.NodesMaker;
using RPGCreator.Core.Parser.Graph.TableHandler;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.Core.Types.Blueprint.Nodes;
using RPGCreator.SDK.Graph;
using Serilog;

namespace RPGCreator.Core.Parser.Graph;

public sealed class GraphTable
{
    public static bool AlreadyScanned { get; private set; } = false;

    private static readonly Dictionary<EGraphOpCode, IGraphInstrHandler> HandlersTable = new();

    /// <summary>
    /// Scan the assemblies for all graph instruction handlers and register them.<br/>
    /// An instruction handlers is a class that implements <see cref="IGraphInstrHandler"/> and has the <see cref="OpcodeAttribute"/> attribute.<br/>
    /// The <see cref="OpcodeAttribute"/> attribute specifies the opcode that the handler is responsible for.<br/>
    /// The handler must also implement the <see cref="IGraphInstrHandler.Exec(GraphInstr, GraphEvalEnvironment, GraphInterpreter)"/> method to execute the instruction.<br/>
    /// And the <see cref="IGraphInstrHandler.Signature"/> property to specify the operand signature of the instruction.<br/>
    /// The <see cref="GraphTable"/> will automatically scan the assemblies for all instruction handlers and register them in the table.
    /// </summary>
    /// <remarks>
    /// The handler also need to:
    /// - Be a sealed class.
    /// - Be a non-abstract class.
    /// - Be a class (not an interface or struct).
    /// </remarks>
    public static void ScanAssemblies()
    {
        if (AlreadyScanned)
        {
            Log.Warning("GraphTable: Already scanned assemblies, skipping.");
            return;
        }
        Log.Information("GraphTable: Scanning assemblies for graph instruction handlers...");
        var assemblies = Assembly.GetExecutingAssembly();

        foreach (var type in assemblies.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false, IsSealed: true }))
        {
            var attr = type.GetCustomAttribute<OpcodeAttribute>();
            if (attr == null)
                continue;
            if (!typeof(IGraphInstrHandler).IsAssignableFrom(type))
                continue;
            
            var handler = (IGraphInstrHandler)Activator.CreateInstance(type)!;
            
            Log.Information("GraphTable: Found handler {Handler} for opcode {OpCode}, trying to register...",
                type.Name, attr.OpCode);
            
            if (HandlersTable.TryGetValue(attr.OpCode, out IGraphInstrHandler? value))
            {
                Log.Error("GraphTable: Opcode {OpCode} is already registered by {ExistingHandler}.", 
                    attr.OpCode, value.GetType().Name);
                continue;
            }
            HandlersTable[attr.OpCode] = handler;
            Log.Information("GraphTable: Registered handler {Handler} for opcode {OpCode}.",
                type.Name, attr.OpCode);
        }
        AlreadyScanned = true;
        Log.Information("GraphTable: Scanned {Count} handlers.", HandlersTable.Count);
    }
    
    /// <summary>
    /// This method checks if all registered handlers have a corresponding node in the <see cref="GraphNodeRegistry"/>.<br/>
    /// This also checks if all nodes have a corresponding handler in the <see cref="HandlersTable"/>.<br/>
    /// It should be called after the <see cref="ScanAssemblies"/> method and after the <see cref="GraphNodeRegistry.AnalyzeNodes"/> method.<br/>
    /// If the <see cref="GraphNodeRegistry"/> is not analyzed yet, it will log a warning and skip the check.<br/>
    /// If the <see cref="HandlersTable"/> is not scanned yet, it will log a warning and skip the check.
    /// </summary>
    /// <remarks>
    /// This method is put here (<see cref="GraphTable"/>) because I don't really know where to put it.<br/>
    /// It is not really a table, but it is related to the graph instructions and the graph nodes.<br/>
    /// If you have a better idea of where to put it, feel free to move it.
    /// </remarks>
    public static void CheckHandlersAndNodesConsistency()
    {
        if (!AlreadyScanned)
        {
            Log.Warning("GraphTable: Not scanned yet, skipping check.");
            return;
        }

        if (!GraphNodeRegistry.AlreadyAnalyzed)
        {
            Log.Warning("GraphTable: GraphNodeRegistry not analyzed yet, skipping check.");
            return;
        }
        
        var handlersToCheck = HandlersTable.Values
            .Where(h => h.GetType().GetCustomAttribute<OpcodeAttribute>().HandlerType != EOpCodeHandlerType.Internal)
            .ToList();

        var opCodesToCheck = handlersToCheck
            .Select(h => h.GetType().GetCustomAttribute<OpcodeAttribute>().OpCode)
            .ToList();
        
        var nodes = GraphNodeRegistry.GetAllNodes()
            .Where(n => n.OpCode != EGraphOpCode.none)
            .ToList();
        
        var nodesOpCodes = nodes
            .Select(n => n.OpCode)
            .ToList();
        
        Log.Information("GraphTable: Checking {Count} handlers against {NodeCount} nodes.",
            handlersToCheck.Count, nodesOpCodes.Count);
        
        var missingHandlers = nodesOpCodes
            .Where(opCode => !opCodesToCheck.Contains(opCode))
            .ToList();
        
        var missingNodes = opCodesToCheck
            .Where(opCode => !nodesOpCodes.Contains(opCode))
            .ToList();
        
        // Remove the 'start' and 'end' opcodes from the missing handlers and nodes lists
        // as they are not supposed to be registered in the handlers table.
        missingHandlers.RemoveAll(opCode => opCode is EGraphOpCode.start or EGraphOpCode.end);
        
        if (missingHandlers.Count > 0)
        {
            Log.Error("GraphTable: The following opcodes are missing handlers: {MissingHandlers}.",
                string.Join(", ", missingHandlers));
        }
        
        if (missingNodes.Count > 0)
        {
            Log.Error("GraphTable: The following opcodes are missing nodes: {MissingNodes}.",
                string.Join(", ", missingNodes));
        }
    }

    /// <summary>
    /// Allow to register a new graph instruction handler.<br/>
    /// See <see cref="ScanAssemblies"/> for more information on how to create a handler.<br/>
    /// <br/>
    /// This method is used to register a handler manually, for example if you want to register a handler that is not in the current assembly.<br/>
    /// It is recommended to use <see cref="ScanAssemblies"/> to automatically register all handlers in the current assembly.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    public static void Register(IGraphInstrHandler handler)
    {
        var attr = handler.GetType().GetCustomAttribute<OpcodeAttribute>();
        if (attr == null)
            return;
        if (HandlersTable.TryGetValue(attr.OpCode, out IGraphInstrHandler? value))
        {
            Log.Error("GraphTable: Opcode {OpCode} is already registered by {ExistingHandler}.", 
                attr.OpCode, value.GetType().Name);
            return;
        }
        HandlersTable[attr.OpCode] = handler;
        Log.Information("GraphTable: Registered handler {Handler} for opcode {OpCode}.",
            handler.GetType().Name, attr.OpCode);
    }

    public static bool IsOpcodeRegistered(EGraphOpCode opCode) => HandlersTable.ContainsKey(opCode);
    public static IGraphInstrHandler Get(EGraphOpCode opCode) => HandlersTable[opCode];
    public static IReadOnlyList<EGraphOpCode> ValidOpcodes => HandlersTable.Keys.ToList();
}