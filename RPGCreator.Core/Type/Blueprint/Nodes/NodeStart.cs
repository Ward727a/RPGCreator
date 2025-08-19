using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Type.Blueprint;

namespace RPGCreator.Core.Type.Blueprint.Nodes;

public class NodeStart : Node
{
    public override EGraphOpCode Type => EGraphOpCode.start;
    public override string Title { get; protected set; } = "Start";

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