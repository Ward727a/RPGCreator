using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Types.Blueprint.Nodes.Skills.Effects;

[GraphNode]
[GraphNodeContext(EGraphNodeContext.SkillEffect)]
public class SE_GetCaster : Node
{
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.get_variable;
    public override string DisplayName { get; protected set; } = "Get Caster";
    public override string Path { get; protected set; } = "Skill Effect|Getters";
    
    public SE_GetCaster()
    {
        Outputs.Add(new Port()
        {
            Name = "Caster",
            AllowManualInput = false,
            Kind = PortKind.Object,
            ObjectInternalType = typeof(SDK.ECS.Entities.Entity)
        });
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        var dst = context.NewRegister();
        var instrs = new List<GraphInstr>();

        // Bind the output value to the register
        context.BindOuput(this, Outputs[0].Id, dst);
        // Allocate the register as near as possible to the node that will consume it
        // This will add the instruction to the list of instructions that will be executed before the node that consumes it
        context.AllocateRegister(
            this,
            Outputs[0].Id,
            dst,
            new List<GraphInstr>{
                GraphIR.Op(EGraphOpCode.get_variable, GraphIR.Operands(EGraphOperandKind.Path, "skill_effect.caster"), GraphIR.Operands(EGraphOperandKind.Register, dst))
            });
        return instrs;
    }
}