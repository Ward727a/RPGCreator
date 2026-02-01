using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Entity;

[Opcode(EGraphOpCode.check_is_type)]
public sealed class CheckIsEntity : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var obj = interpreter.EvalRegisterOperand<object?>(instr.Operands[0]);
        var to = interpreter.ParseRegisterOperand(instr.Operands[1]);

        env.SetRegister(to, obj is SDK.ECS.Entities.Entity);
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.Register, EGraphOperandKind.Register];
}