using RPGCreator.Core.Common;
using RPGCreator.Core.Parser.Graph;
using RPGCreator.SDK.Graph;
using Serilog;

namespace RPGCreator.Core.Types.Blueprint;

public sealed class GraphDocumentCompiler(GraphDocument doc)
{
    private GraphDocument Graph { get; } = doc;
    private GraphCompileContext CompileContext { get; } = new();
    private GraphAllocator Allocator;
    public string? FormattedValue { get; private set; }
    
    /// <summary>
    /// Describe the inheritance of nodes, where the key is the "parent" node ID and the value is a list of child node IDs that inherit from it.
    /// This is used to determine what nodes should be first in the execution order, as well as to handle inheritance of properties and methods.
    /// </summary>
    private Dictionary<string, HashSet<string>> _nodeDependencyGraph = new();
    private List<string> _sortedNodes = new();
    
    private List<string> _pureNodesExecutionOrder = new();
    
    private List<GraphLabeledInstr> _program = new();

    public static GraphDocumentCompiled Compile(GraphDocument doc, bool shouldSaveTemp = true)
    {
        var compiler = new GraphDocumentCompiler(doc);
        try
        {
            compiler.Format();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while compiling the graph document: {Message}", e.Message);
            
            return new GraphDocumentCompiled(new List<GraphLabeledInstr>(), doc.GraphVariables.ToDictionary()){ DocumentPath = doc.SavePath};
        }

        if(shouldSaveTemp && string.IsNullOrWhiteSpace(doc.SavePath))
        {
            // Generate a temporary path for saving the document
            var path = Path.Combine(Path.GetTempPath(), $"graph_temp_{Ulid.NewUlid()}.rpg.bp");
            doc.Save(path);
        }
        return new GraphDocumentCompiled(compiler._program, doc.GraphVariables.ToDictionary()){ DocumentPath = doc.SavePath };
    }
    
    public void Format()
    {
        if (!BuildInheritanceGraph())
        {
            Log.Error("Failed to build inheritance graph. Circular dependencies detected or invalid graph structure.");
            FormattedValue = null;
            return;
        }
        Log.Information("Inheritance graph built successfully with {Count} nodes.", _sortedNodes.Count);
        // Log the sorted nodes titles for debugging
        Log.Debug("Sorted nodes: {Nodes}", string.Join(", ", _sortedNodes.Select(id => $"{id} ({Graph.Nodes[id].DisplayName})")));
        
        Log.Information("Validating nodes in the graph...");
        ValidateNodes();
        
        Log.Information("Nodes validated successfully. {Count} nodes can be executed.", _sortedNodes.Count);
        
        // Now we can start compiling the nodes
        _program.Clear();
        
        foreach (var nodeId in _sortedNodes)
        {
            var node = Graph.Nodes[nodeId];
            Log.Information("Compiling node {NodeId} ({NodeTitle})...", nodeId, node.DisplayName);

            if (node.OpCode is EGraphOpCode.start or EGraphOpCode.comment or EGraphOpCode.comment
                or EGraphOpCode.none or EGraphOpCode.end)
            {
                Log.Information("Node {NodeId} ({NodeTitle}) is a special node of code {OPCode} and will not be compiled.", nodeId, node.DisplayName, node.OpCode);
                continue; // Skip special nodes that do not need compilation
            }
            
            // Compile the properties of the node
            var instrs = node.Emit(Graph, CompileContext).ToList();
            if (!instrs.Any())
            {
                Log.Warning("Node {NodeId} ({NodeTitle}) has no instructions to execute (Pure?).", nodeId, node.DisplayName);
                continue; // Skip nodes with no instructions
            }
            
            if (instrs.Count > 0)
            {
                if (IsPure(node))
                {
                    Log.Error("Node {NodeId} ({NodeTitle}) is pure and as such should not be added to the instructions list directly. It should be linked to another node.", nodeId, node.DisplayName);
                    Log.Error("This is a bug in the graph compiler, please report it to the developers.");
                    Log.Error("The graph can't be compiled due to this issue.");
                    FormattedValue = null;
                    return;
                }

                var label = CreateLabel(node);

                if (label != null)
                {
                    _program.Add(GraphIR.Label(label, instrs));
                    Log.Information("Node {NodeId} ({NodeTitle}) compiled successfully with label {Label}.", nodeId, node.DisplayName, label);
                    continue;
                }
                
                Log.Error("Node {NodeId} ({NodeTitle}) generated an empty or null label. This is a bug in the graph compiler, please report it to the developers.", nodeId, node.DisplayName);
                Log.Error("The graph can't be compiled due to this issue.");
                FormattedValue = null;
                return;
            }
            
            Log.Information("Node {NodeId} ({NodeTitle}) compiled but has no instructions to execute.", nodeId, node.DisplayName);
            Log.Information("Is this intended? If not, please check node implementation and ensure it Emits instructions correctly.");
        }

        Log.Information("Starting to optimize register allocation for the instructions...");
        
        Allocator = new(_program);
        Allocator.OptimizeRegisterAllocation(); // Optimize the register allocation for the instructions

        if (Allocator.FoundInvalidRegister)
        {
            Log.Error("Invalid register found during allocation. Some instructions may not be valid.");
            Log.Error("Impossible to compile the graph due to invalid register allocation.");
            Log.Error("If this is not a modified version of the engine, please report this issue to the developers.");
            Log.Error("Otherwise, please check with the creator of this modified engine to see if they have made any changes that could cause this issue.");
            FormattedValue = null;
            return;
        }
        
        Log.Information("Register allocation optimized successfully");
        
        Log.Information("Compiling instruction program to bytecode...");
        // Compile the instructions to bytecode
        
        File.WriteAllText("test_bp.rpg.bp",new GraphDocumentWriter(_program).Write());
        // return;
        
        // Add a fake player.name to the environment, this is used to get the player name in the graph
        GraphEvalEnvironment.AddVM("player.name", "Player Name");
        
        GraphInterpreter interpreter = new(_program, new GraphEvalEnvironment());
        Log.Information("Running the graph interpreter...");
        try
        {
            interpreter.Run();
            Log.Information("Graph interpreter ran successfully.");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while running the graph interpreter.");
            Log.Error("The graph can't be compiled due to this issue.");
            FormattedValue = null;
        }
    }

    private bool BuildInheritanceGraph()
    {
        _nodeDependencyGraph.Clear();
        _sortedNodes.Clear();

        var links = new List<(string from, string to)>();
        
        // We first check if any link is incompatible, and log the errors.
        // If any incompatible link is found, we return false and do not build the graph.
        var nonCompatibleLinks = Graph.Links.Where(link => !IsCompatibleLink(link)).ToList();
        if (nonCompatibleLinks.Any())
        {
            foreach (var nonCompatibleLink in nonCompatibleLinks)
            {
                Log.Error("Link from {FromNodeId} to {ToNodeId} is not compatible.", nonCompatibleLink.FromNodeId, nonCompatibleLink.ToNodeId);
            }
            return false; // Incompatible link found
        }
        
        foreach (var link in Graph.Links)
        {
            links.Add((link.FromNodeId, link.ToNodeId));
        }
        
        _nodeDependencyGraph = DependencyGraph.BuildGraph(links);

        try
        {
            _sortedNodes = DependencyGraph.TopologicalSort(_nodeDependencyGraph);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while sorting nodes in the dependency graph.");
            return false;
        }
    }

    /// <summary>
    /// Here we validate the nodes in the graph to ensure that they are correctly set up for execution.
    /// </summary>
    private void ValidateNodes()
    {
        foreach ((string id, Node node) in Graph.Nodes)
        {
            if (!_sortedNodes.Contains(id))
            {
                if (IsParentless(node))
                {
                    Log.Warning("Node {NodeId} is parentless and will not be executed: {NodeTitle}", id, node.DisplayName);
                }
            }
        }
    }

    /// <summary>
    /// Find if a port has a link, and return the from and to node IDs if it does.
    /// </summary>
    /// <param name="portId">The ID of the port to check for links.</param>
    /// <param name="fromNode">The ID of the node from which the link originates.</param>
    /// <param name="toNode">The ID of the node to which the link points.</param>
    /// <returns></returns>
    private bool HasLink(string portId, out string fromNode, out string toNode)
    {
        if(Graph.Links.Any(l => l.FromPortId == portId || l.ToPortId == portId))
        {
            var link = Graph.Links.First(l => l.FromPortId == portId || l.ToPortId == portId);
            
            fromNode = link.FromNodeId;
            toNode = link.ToNodeId;
            
            return true;
        }

        fromNode = "";
        toNode = "";

        return false;
    }
    
    /// <summary>
    /// This check if a link is compatible:<br/>
    /// If the link is from an execution port, it must be to an execution port.<br/>
    /// If the link is from a string port, it can be to string, number, or boolean ports.<br/>
    /// If the link is from a number port, it can be to string, number, or boolean ports.<br/>
    /// If the link is from a boolean port, it can be to string, number, or boolean ports.<br/>
    /// If the link is from a data port, it can be to string, or data ports.<br/>
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    private bool IsCompatibleLink(Link link)
    {
        var fromNode = GetNode(link.FromNodeId);
        var toNode = GetNode(link.ToNodeId);
        
        var fromPort = fromNode.Outputs.FirstOrDefault(p => p.Id == link.FromPortId);
        var toPort = toNode.Inputs.FirstOrDefault(p => p.Id == link.ToPortId);
        
        if(fromPort == null || toPort == null)
        {
            Log.Error("Link from port {FromPortId} to port {ToPortId} is invalid: one of the ports does not exist.", link.FromPortId, link.ToPortId);
            return false; // Invalid link if either port does not exist
        }

        // If they are both the same kind, they are compatible by default
        if (fromPort.Kind == toPort.Kind)
            return true;
        
        switch (fromPort.Kind)
        {
            case PortKind.Exec:
                return toPort.Kind == PortKind.Exec;
            case PortKind.String:
            case PortKind.Number:
            case PortKind.Boolean:
                return (toPort.Kind == PortKind.String || 
                        toPort.Kind == PortKind.Number || 
                        toPort.Kind == PortKind.Boolean);
            case PortKind.Data:
                return toPort.Kind == PortKind.String;
            case PortKind.Value:
            case PortKind.Events:
            case PortKind.Object:
            default:
                return false; // Unsupported port kind
        }
    }

    /// <summary>
    /// Check if a node is "pure", meaning it does not have any execution port.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool IsPure(Node node)
    {
        return !(node.Inputs.Any(p => p.Kind == PortKind.Exec) ||
                 node.Outputs.Any(p => p.Kind == PortKind.Exec));
    }
    
    private bool IsParentless(Node node)
    {
        if (IsPure(node)) // A pure node can't be parentless as it has no execution ports
            return false;
        // A node is parentless if it has no incoming links
        return !Graph.Links.Any(l => l.ToNodeId == node.Id);
    }

    private Node GetNode(string id)
    {
        if (Graph.Nodes.TryGetValue(id, out var node))
        {
            return node;
        }

        throw new KeyNotFoundException($"Node with ID {id} not found in the graph.");
    }

    private string? CreateLabel(Node node)
    {

        var label = node.Id.Replace(" ", "_").ToLowerInvariant();
        if (!string.IsNullOrEmpty(label)) return $"{label}:";
        
        Log.Error("Node {NodeId} generated an empty or null label (title: {NodeTitle}).", node.Id, node.DisplayName);
        
        return null;

    }
    
    private void DEBUG_SaveInstructions(string filePath)
    {
        // // This method is for debugging purposes, to save the instructions to a file.
        // // It can be used to check the generated code and ensure it is correct.
        //
        // StringBuilder sb = new StringBuilder();
        // sb.AppendLine("// Generated Instructions");
        // sb.AppendLine("// This file is generated for debugging purposes and should not be used in production.");
        // sb.AppendLine("// If you find any issues, please report them to the developers.");
        //
        // foreach (var instr in _instructions)
        // {
        //     sb.Append(instr.Op);
        //     
        //     if (instr.Args != null && instr.Args.Any())
        //     {
        //         sb.AppendLine(" " + string.Join(" ", instr.Args));
        //     }
        //     else
        //     {
        //         sb.AppendLine();
        //     }
        // }
        // sb.AppendLine("// End of instructions");
        // sb.AppendLine("// Total instructions: " + _instructions.Count);
        // sb.AppendLine("// Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //
        // File.WriteAllText(filePath, sb.ToString());
        //
        // Log.Information("Instructions saved to {FilePath}", filePath);
    }

}