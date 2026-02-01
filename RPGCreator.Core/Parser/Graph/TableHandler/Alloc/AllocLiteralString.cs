using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Alloc;

[Opcode(EGraphOpCode.alloc_literal_string, EOpCodeHandlerType.Internal)]
public sealed class AllocLiteralString : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.ParseStringOperand(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);
        env.SetRegister(dest, value);
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.LiteralString, EGraphOperandKind.Register];
}