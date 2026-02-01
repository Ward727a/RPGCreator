using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.GetSet;

[Opcode(EGraphOpCode.get_variable)]
public sealed class GetVariable : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var from = interpreter.ParsePathOperand(instr.Operands[0]);
        var to = interpreter.ParseRegisterOperand(instr.Operands[1]);

        env.SetVariable("skill_effect.caster", new SDK.ECS.Entities.Entity()); // TEST ONLY
        env.SetRegister(to, env.GetVariable(from));
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.Path, EGraphOperandKind.Register];
}