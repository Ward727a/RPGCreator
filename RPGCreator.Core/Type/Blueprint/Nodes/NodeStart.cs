using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Parser.Graph.NodesMaker;
using RPGCreator.Core.Type.Blueprint;

namespace RPGCreator.Core.Type.Blueprint.Nodes;

[GraphNode]
public class NodeStart : Node
{
    public override EGraphOpCode OpCode => EGraphOpCode.start;
    public override string Description => "This is the start node of the graph. It is the entry point for execution.";
    // A path with "@hide" indicates that this node is not meant to be displayed in the menu.
    public override string Path => "@hide";
    public override string DisplayName { get; protected set; } = "Start";

    public NodeStart()
    {
        // The start node does not have any outputs, as it is the entry point of the graph.
        Outputs.Add(new Port() { Kind = PortKind.Exec, AllowManualInput = false, Name = "Out"});
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        throw new InvalidOperationException("Start node can't emit any instruction!");
    }
}