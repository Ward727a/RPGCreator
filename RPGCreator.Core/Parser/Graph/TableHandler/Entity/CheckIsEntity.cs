using RPGCreator.Core.Types.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Entity;

[Opcode(EGraphOpCode.check_is_type)]
public sealed class CheckIsEntity : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var obj = interpreter.EvalRegisterOperand<object?>(instr.Operands[0]);
        var to = interpreter.ParseRegisterOperand(instr.Operands[1]);

        env.SetRegister(to, obj is Runtimes.ECS.Entity);
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.Register, EGraphOperandKind.Register];
}