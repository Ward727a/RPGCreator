using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_round)]
public sealed class MathRound : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = (double)(interpreter.EvalOperand(instr.Operands[0]) ?? 0);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);

        env.Registers[dest] = System.Math.Round(value);
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register];
}