using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Types.Blueprint.Nodes.Gets;

[GraphNode]
public class GetPlayerName : Node
{
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.get_vm;
    public override string DisplayName { get; protected set; } = "Get Player Name";

    public override string Description => "Get the name of the player character from the game state.";
    public override string Path  { get; protected set; } = "Player|Getters";

    public GetPlayerName()
    {
        Outputs.Add(new Port(){ Kind = PortKind.String, AllowManualInput = false, Name = "Name"});
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        // For now this will be a "fake" Emit and variable!
        var instrs = new List<GraphInstr>();
        var dst = context.NewRegister();
        context.BindOuput(this, Outputs[0].Id, dst);
        context.AllocateRegister(this, Outputs[0].Id, dst, new List<GraphInstr>{
            GraphIR.Op(EGraphOpCode.get_vm, GraphIR.Operands(EGraphOperandKind.Path, "player.name"), GraphIR.Operands(EGraphOperandKind.Register, dst))
        });
        return instrs;
    }
}