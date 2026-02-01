using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_negate)]
public sealed class MathNegate : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalRegisterOperand<double>(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);

        env.SetRegister(dest, -value);
    }

    public EGraphOperandKind[] Signature { get; } 
        = [EGraphOperandKind.Register, EGraphOperandKind.Register];
}