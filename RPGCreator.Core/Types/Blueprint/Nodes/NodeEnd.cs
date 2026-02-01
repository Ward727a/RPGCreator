using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Types.Blueprint.Nodes;

[GraphNode]
public class NodeEnd : Node
{
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.end;
    public override string Description  { get; protected set; } = "End the execution of the current graph.";
    public override string Path  { get; protected set; } = "@hide";
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
        throw new InvalidOperationException("End node can't Emit any instruction!");
    }
}