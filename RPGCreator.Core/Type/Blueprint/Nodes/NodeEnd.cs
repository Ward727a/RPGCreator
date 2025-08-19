using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Type.Blueprint;

namespace RPGCreator.Core.Type.Blueprint.Nodes;

public class NodeEnd : Node
{
    public override EGraphOpCode Type => EGraphOpCode.end;
    public override string Title { get; protected set; } = "End";

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