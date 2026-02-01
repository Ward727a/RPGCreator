using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Types.Blueprint.Nodes;

[GraphNode]
public class NodeStart : Node
{
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.start;
    public override string Description  { get; protected set; } = "This is the start node of the graph. It is the entry point for execution.";
    // A path with "@hide" indicates that this node is not meant to be displayed in the menu.
    public override string Path  { get; protected set; } = "@hide";
    public override string DisplayName { get; protected set; } = "Start";

    public NodeStart()
    {
        // The start node does not have any outputs, as it is the entry point of the graph.
        Outputs.Add(new Port() { Kind = PortKind.Exec, AllowManualInput = false, Name = "Begin"});
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        throw new InvalidOperationException("Start node can't Emit any instruction!");
    }
}