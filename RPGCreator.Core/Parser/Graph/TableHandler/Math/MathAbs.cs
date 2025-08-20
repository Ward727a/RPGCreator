using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_abs)]
public sealed class MathAbs : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalOperand(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);

        if (value is double dValue)
        {
            env.Registers[dest] = System.Math.Abs(dValue);
        }
        else if (value is int iValue)
        {
            env.Registers[dest] = System.Math.Abs(iValue);
        }
        else
        {
            throw new InvalidOperationException("Unsupported operand type for abs operation.");
        }
    }

    public EGraphOperandKind[] Signature { get; } 
        = [EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register];
}