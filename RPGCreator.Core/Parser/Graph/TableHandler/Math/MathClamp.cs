using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_clamp)]
public sealed class MathClamp : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalRegisterOperand<double>(instr.Operands[0]);
        var min = interpreter.EvalRegisterOperand<double>(instr.Operands[1]);
        var max = interpreter.EvalRegisterOperand<double>(instr.Operands[2]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[3]);

        env.SetRegister(dest, System.Math.Clamp(value, min, max));
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register, 
           EGraphOperandKind.Register, 
           EGraphOperandKind.Register, 
           EGraphOperandKind.Register];
}