using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_negate)]
public sealed class MathNegate : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalOperand(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);

        if (value is double d)
        {
            env.Registers[dest] = -d;
        }
        else if (value is int i)
        {
            env.Registers[dest] = -i;
        }
        else
        {
            throw new InvalidOperationException($"Unsupported operand type for negation: {value?.GetType()}");
        }
    }

    public EGraphOperandKind[] Signature { get; } 
        = [EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register];
}