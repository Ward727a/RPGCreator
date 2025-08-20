using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_sqrt)]
public sealed class MathSqrt : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalOperand(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);

        if (value is double dValue)
        {
            env.Registers[dest] = System.Math.Sqrt(dValue);
        }
        else if (value is int iValue)
        {
            env.Registers[dest] = System.Math.Sqrt(iValue);
        }
        else
        {
            throw new InvalidOperationException("Unsupported operand type for sqrt operation.");
        }
    }

    public EGraphOperandKind[] Signature { get; } 
        = [EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register];
}