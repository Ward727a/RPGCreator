using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_min)]
public sealed class MathMin : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var a = (double)(interpreter.EvalOperand(instr.Operands[0]) ?? 0);
        var b = (double)(interpreter.EvalOperand(instr.Operands[1]) ?? 0);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[2]);

        env.Registers[dest] = System.Math.Min(a, b);
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register];
}