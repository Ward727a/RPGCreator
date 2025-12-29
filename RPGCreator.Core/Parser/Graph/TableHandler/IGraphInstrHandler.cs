using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph.TableHandler;

public interface IGraphInstrHandler
{
    void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter);
    EGraphOperandKind[] Signature { get; }
}