using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_pow)]
public sealed class MathPow : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var baseValue = interpreter.EvalRegisterOperand<double>(instr.Operands[0]);
        var exponent = interpreter.EvalRegisterOperand<double>(instr.Operands[1]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[2]);

        env.SetRegister(dest, System.Math.Pow(baseValue, exponent));
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register, EGraphOperandKind.Register, EGraphOperandKind.Register];
}