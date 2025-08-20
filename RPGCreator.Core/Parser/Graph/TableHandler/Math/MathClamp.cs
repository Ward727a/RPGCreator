using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_clamp)]
public sealed class MathClamp : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = (double)(interpreter.EvalOperand(instr.Operands[0]) ?? 0);
        var min = (double)(interpreter.EvalOperand(instr.Operands[1]) ?? 0);
        var max = (double)(interpreter.EvalOperand(instr.Operands[2]) ?? 0);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[3]);

        env.Registers[dest] = System.Math.Clamp(value, min, max);
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register | EGraphOperandKind.Literal, 
           EGraphOperandKind.Register | EGraphOperandKind.Literal, 
           EGraphOperandKind.Register | EGraphOperandKind.Literal, 
           EGraphOperandKind.Register];
}