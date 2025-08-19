using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Media;
using RPGCreator.Core;
using RPGCreator.Core.Common;
using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Type.Blueprint.Nodes;
using Serilog;

namespace RPGCreator.Core.Type.Blueprint;

public sealed class GraphDocument
{
    public readonly Dictionary<string, Node> Nodes = new();
    public readonly List<Link> Links = new();

    public event Action<Node>? NodeAdded;
    public event Action<Node>? NodeRemoved;
    public event Action<Link>? LinkAdded;
    public event Action<Link>? LinkRemoved;
    public event Action<Node>? NodeMoved;
    
    public Node AddNode(Node n) { Nodes[n.Id] = n; NodeAdded?.Invoke(n); return n; }
    public void RemoveNode(string id) { if (Nodes.Remove(id, out var n)) NodeRemoved?.Invoke(n); }
    public void MoveNode(string id, double x, double y) { var n = Nodes[id]; n.X=x; n.Y=y; NodeMoved?.Invoke(n); }
    public void AddLink(Link l) { Links.Add(l); LinkAdded?.Invoke(l); }
    public void RemoveLink(Link l) { Links.Remove(l); LinkRemoved?.Invoke(l); }

    public void Compile()
    {
        
    }

    public void Save(string toFile)
    {
        // Serialize nodes to JSON
        new GraphDocumentCompiler(this).Format();
    }
}

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
        Log.Debug("Sorted nodes: {Nodes}", string.Join(", ", _sortedNodes.Select(id => $"{id} ({Graph.Nodes[id].Title})")));
        
        Log.Information("Validating nodes in the graph...");
        ValidateNodes();
        
        Log.Information("Nodes validated successfully. {Count} nodes can be executed.", _sortedNodes.Count);
        
        // Now we can start compiling the nodes
        _program.Clear();
        
        foreach (var nodeId in _sortedNodes)
        {
            var node = Graph.Nodes[nodeId];
            Log.Information("Compiling node {NodeId} ({NodeTitle})...", nodeId, node.Title);

            if (node.Type is EGraphOpCode.start or EGraphOpCode.comment or EGraphOpCode.comment
                or EGraphOpCode.none or EGraphOpCode.end)
            {
                Log.Information("Node {NodeId} ({NodeTitle}) is a special node of code {OPCode} and will not be compiled.", nodeId, node.Title, node.Type);
                continue; // Skip special nodes that do not need compilation
            }
            
            // Compile the properties of the node
            var instrs = node.Emit(Graph, CompileContext).ToList();
            if (!instrs.Any())
            {
                Log.Warning("Node {NodeId} ({NodeTitle}) has no instructions to execute.", nodeId, node.Title);
                continue; // Skip nodes with no instructions
            }
            
            if (instrs.Count > 0)
            {
                if (IsPure(node))
                {
                    Log.Error("Node {NodeId} ({NodeTitle}) is pure and as such should not be added to the instructions list directly. It should be linked to another node.", nodeId, node.Title);
                    Log.Error("This is a bug in the graph compiler, please report it to the developers.");
                    Log.Error("The graph can't be compiled due to this issue.");
                    FormattedValue = null;
                    return;
                }

                var label = CreateLabel(node);

                if (label != null)
                {
                    _program.Add(GraphIR.Label(label, instrs));
                    Log.Information("Node {NodeId} ({NodeTitle}) compiled successfully with label {Label}.", nodeId, node.Title, label);
                    continue;
                }
                
                Log.Error("Node {NodeId} ({NodeTitle}) generated an empty or null label. This is a bug in the graph compiler, please report it to the developers.", nodeId, node.Title);
                Log.Error("The graph can't be compiled due to this issue.");
                FormattedValue = null;
                return;
            }
            
            Log.Information("Node {NodeId} ({NodeTitle}) compiled but has no instructions to execute.", nodeId, node.Title);
            Log.Information("Is this intended? If not, please check node implementation and ensure it emits instructions correctly.");
        }

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
        
        DEBUG_SaveInstructions("debug_instructions.txt"); // Save the instructions to a file for debugging purposes
        
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
                    Log.Warning("Node {NodeId} is parentless and will not be executed: {NodeTitle}", id, node.Title);
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
        
        Log.Error("Node {NodeId} generated an empty or null label (title: {NodeTitle}).", node.Id, node.Title);
        
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

public abstract class Node
{
    public string Id { get; } = Ulid.NewUlid().ToString();
    public abstract EGraphOpCode Type { get; }
    public abstract string Title { get; protected set; }
    public double X, Y;
    public readonly List<Port> Inputs = new();
    public readonly List<Port> Outputs = new();

    protected IReadOnlyCollection<Port> GetOutputs(string name)
    {
        return Outputs.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
    }
    
    protected Port TryGetOuput(string name)
    {
        return Outputs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new KeyNotFoundException($"Output port with name '{name}' not found.");
    }
    
    protected IReadOnlyCollection<Port> GetInputs(string name)
    {
        return Inputs.Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList().AsReadOnly();
    }
    
    protected Port TryGetInput(string name)
    {
        return Inputs.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? throw new KeyNotFoundException($"Input port with name '{name}' not found.");
    }
    
    /// <summary>
    /// A properties dictionary that will be used to store the properties of the node.<br/>
    /// The properties here are used to store the inputs/outputs of the node to be used in the compiled code.<br/>
    /// These properties can be edited by the user in the UI, or set by other nodes.<br/>
    /// Please, note that even a small change in how the properties are made can lead to a complete crash of the engine.
    /// </summary>
    public Dictionary<string, object?> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// This method is called when the node is compiled to generate the properties that will be used in the compiled code.<br/>
    /// It should be overridden in derived classes to add specific properties.<br/>
    /// The properties are the inputs, for exemple if we have a node "Add" type: "op_math_add" that have 2 inputs "First input" and "Second input", the properties will be:
    /// <code>
    /// {
    ///     "A": 0, // First input, named "A" because this is what the type "op.math.add" expects
    ///     "B": 0 // Second input, named "B" because this is what the type "op.math.add" expects
    /// }
    /// </code>
    /// To be certains of what properties are expected, you can check the ENodeOpCode enum, it contains the expected properties for each node type.
    /// </summary>
    public abstract IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context);
}

public enum PortKind
{
    Exec,
    Value,
    Events,
    Data,
    String,
    Number,
    Boolean,
    Object,
    Enum
}
public static class PortKindExtensions
{
    public static string ToDisplayString(this PortKind kind) => kind switch
    {
        PortKind.Exec => "Execution",
        PortKind.Value => "Value",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
    public static Color GetColor(this PortKind kind) => kind switch
    {
        PortKind.Exec => Color.FromRgb(255, 165, 0), // Orange for Exec
        PortKind.Value => Color.FromRgb(135, 206, 235), // SkyBlue for Value
        PortKind.Events => Color.FromRgb(255, 192, 203), // Pink for Events
        PortKind.Data => Color.FromRgb(144, 238, 144), // LightGreen for Data
        PortKind.String => Color.FromRgb(255, 255, 224), // LightYellow for String
        PortKind.Number => Color.FromRgb(255, 222, 173), // LightSalmon for Number
        PortKind.Boolean => Color.FromRgb(255, 160, 122), // LightCoral for Boolean
        PortKind.Object => Color.FromRgb(221, 160, 221), // Plum for Object
        PortKind.Enum => Color.FromRgb(100, 149, 237), // CornflowerBlue for Enum
        _ => Color.FromRgb(120, 120, 120) // Default gray for unknown kinds
    };
}

public class Port
{
    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string Name { get; set; } = "";
    public PortKind Kind { get; set; }
    public string ValueType { get; set; } = ""; // "float", "bool", etc.
    public object? Value { get; set; } = ""; // String representation of the value, e.g. "42" for a float or "true" for a bool.
    public bool IsInput { get; set; } = false;
    public bool AllowManualInput { get; set; } = true; // Whether the user can manually input a value in the UI
}

public class StringPort : Port
{
    public StringPort()
    {
        Kind = PortKind.String;
        ValueType = "string";
        Value = "";
    }
    
    private string _value = string.Empty;
    public new string Value
    {
        get => _value;
        set
        {
            if (value != _value)
            {
                _value = value;
            }
        }
    }
}

public class NumberPort : Port
{
    public NumberPort()
    {
        Kind = PortKind.Number;
        ValueType = "double";
        Value = 0D;
    }
    
    private double _value = 0D;
    public new double Value
    {
        get => _value;
        set
        {
            if (value != _value)
            {
                _value = value;
            }
        }
    }
}

public class BooleanPort : Port
{
    public BooleanPort()
    {
        Kind = PortKind.Boolean;
        ValueType = "bool";
        Value = false;
    }
    
    private bool _value = false;
    public new bool Value
    {
        get => _value;
        set
        {
            if (value != _value)
            {
                _value = value;
            }
        }
    }
}

public class EnumPort : Port
{
    public System.Type EnumType { get; private set; }
    public EnumPort(System.Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("The provided type must be an enum type.", nameof(enumType));
        
        Kind = PortKind.Enum;
        ValueType = enumType.Name;
        EnumType = enumType;
        Value = Enum.GetValues(enumType).GetValue(0); // Default to the first value of the enum
    }
}

public sealed class Link
{
    public string FromNodeId { get; private set; }
    public string ToNodeId { get; private set; }
    public string FromPortId { get; private set; }
    public string ToPortId { get; private set; }
    
    public Link(string fromNodeId, string fromPortId, string toNodeId, string toPortId)
    {
        FromNodeId = fromNodeId;
        FromPortId = fromPortId;
        ToNodeId = toNodeId;
        ToPortId = toPortId;
    }
}
