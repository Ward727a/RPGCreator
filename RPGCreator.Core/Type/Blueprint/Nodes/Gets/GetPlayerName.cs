using RPGCreator.Core.Parser.Graph;
using RPGCreator.Core.Type.Blueprint;
using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Type.Blueprint.Nodes.Gets;

public class GetPlayerName : Node
{
    public override EGraphOpCode Type => EGraphOpCode.get_vm;
    public override string Title { get; protected set; } = "Get Player Name";

    public GetPlayerName()
    {
        Outputs.Add(new Port(){ Kind = PortKind.String, AllowManualInput = false, Name = "Name"});
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        // For now this will be a "fake" emit and variable!
        var instrs = new List<GraphInstr>();
        var dst = context.NewRegister();
        context.BindValue(this, Outputs[0].Id, dst);
        context.AllocateRegister(this, Outputs[0].Id, dst, new List<GraphInstr>{
            GraphIR.Op(EGraphOpCode.get_vm, GraphIR.Operands(EGraphOperandKind.Path, "player.name"), GraphIR.Operands(EGraphOperandKind.Register, dst))
        });
        return instrs;
    }
}