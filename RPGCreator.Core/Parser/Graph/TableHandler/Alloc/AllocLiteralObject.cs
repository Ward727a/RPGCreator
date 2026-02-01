using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Alloc;

[Opcode(EGraphOpCode.alloc_literal_object, EOpCodeHandlerType.Internal)]
public sealed class AllocLiteralObject : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalOperand(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);
        if (value == null)
        {
            throw new InvalidOperationException("Cannot allocate a null object.");
        }
        env.SetRegister(dest, value);
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.Literal, EGraphOperandKind.Register];
}