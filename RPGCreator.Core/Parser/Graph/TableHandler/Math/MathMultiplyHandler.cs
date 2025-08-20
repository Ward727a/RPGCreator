using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Math;

[Opcode(EGraphOpCode.math_multiply)]
public sealed class MathMultiplyHandler : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var a = (double)(interpreter.EvalOperand(instr.Operands[0]) ?? 0);
        var b = (double)(interpreter.EvalOperand(instr.Operands[1]) ?? 1);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[2]);
                
        env.Registers[dest] = a * b;
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.Register, EGraphOperandKind.Register, EGraphOperandKind.Register];
}