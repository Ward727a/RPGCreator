using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_divide)]
public sealed class MathDivide : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var a = interpreter.EvalRegisterOperand<double>(instr.Operands[0]);
        var b = interpreter.EvalRegisterOperand<double>(instr.Operands[1]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[2]);

        if (b == 0)
        {
            throw new DivideByZeroException("Division by zero is not allowed.");
        }

        env.SetRegister(dest, a / b);
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register, EGraphOperandKind.Register, EGraphOperandKind.Register];
}