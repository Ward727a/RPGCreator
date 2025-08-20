using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_pow)]
public sealed class MathPow : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var baseValue = interpreter.EvalOperand(instr.Operands[0]);
        var exponent = interpreter.EvalOperand(instr.Operands[1]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[2]);

        if (baseValue is double dBase && exponent is double dExponent)
        {
            env.Registers[dest] = System.Math.Pow(dBase, dExponent);
        }
        else if (baseValue is int iBase && exponent is int iExponent)
        {
            env.Registers[dest] = System.Math.Pow(iBase, iExponent);
        }
        else
        {
            throw new InvalidOperationException("Unsupported operand types for power operation.");
        }
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register];
}