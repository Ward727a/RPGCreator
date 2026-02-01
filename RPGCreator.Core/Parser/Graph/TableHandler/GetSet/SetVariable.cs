using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.GetSet;

[Opcode(EGraphOpCode.set_variable)]
public sealed class SetVariable : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var from = interpreter.ParseRegisterOperand(instr.Operands[0]);
        var to = interpreter.ParsePathOperand(instr.Operands[1]);

        env.SetVariable(to, env.GetRegister(from));
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register, EGraphOperandKind.Path];
}