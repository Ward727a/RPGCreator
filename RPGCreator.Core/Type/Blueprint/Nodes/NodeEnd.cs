using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Parser.Graph.NodesMaker;
using RPGCreator.Core.Type.Blueprint;

namespace RPGCreator.Core.Type.Blueprint.Nodes;

[GraphNode]
public class NodeEnd : Node
{
    public override EGraphOpCode OpCode => EGraphOpCode.end;
    public override string Description => "End the execution of the current graph.";
    public override string Path => "@hide";
    public override string DisplayName { get; protected set; } = "End";

    public NodeEnd()
    {
        Inputs.Add(new Port()
        {
            Kind = PortKind.Exec,
            AllowManualInput = false,
            Name = "Finish",
            IsInput = true
        });
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        throw new InvalidOperationException("End node can't emit any instruction!");
    }
}