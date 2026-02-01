using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_abs)]
public sealed class MathAbs : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalRegisterOperand<double>(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);

        env.SetRegister(dest, System.Math.Abs(value));
    }

    public EGraphOperandKind[] Signature { get; } 
        = [EGraphOperandKind.Register, EGraphOperandKind.Register];
}