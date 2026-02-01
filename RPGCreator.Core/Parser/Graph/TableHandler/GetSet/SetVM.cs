using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler.GetSet;

[Opcode(EGraphOpCode.set_vm)]
public sealed class SetVM : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalOperand(instr.Operands[0]);
        var path = interpreter.ParsePathOperand(instr.Operands[1]);
        
        GraphEvalEnvironment.AddVM(path, value);
        // Note: The value is stored in the environment, not in the registers.
        // This is because VMs are not typically stored in registers.
        // They are here to stay even after the blueprint ends.
        // Example: A VM that store's the player's name or other persistent data.
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Literal | EGraphOperandKind.Register, EGraphOperandKind.Path];
}