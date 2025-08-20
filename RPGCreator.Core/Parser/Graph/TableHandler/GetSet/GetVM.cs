using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph.TableHandler.GetSet;

[Opcode(EGraphOpCode.get_vm)]
public sealed class GetVM : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var path = interpreter.ParsePathOperand(instr.Operands[0]);
        var dest = interpreter.ParseRegisterOperand(instr.Operands[1]);

        env.Registers[dest] = env.GetVM(path);
    }

    public EGraphOperandKind[] Signature { get; } = 
        [EGraphOperandKind.Path, EGraphOperandKind.Register];
}