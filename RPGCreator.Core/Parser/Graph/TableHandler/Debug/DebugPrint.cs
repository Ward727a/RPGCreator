using RPGCreator.Core.Types.Blueprint.Nodes.Debug;
using RPGCreator.SDK.Graph;
using Serilog;

namespace RPGCreator.Core.Parser.Graph.TableHandler.Debug;

[Opcode(EGraphOpCode.debug_print)]
public sealed class DebugPrint : IGraphInstrHandler
{
    public void Exec(GraphInstr instr, GraphEvalEnvironment env, GraphInterpreter interpreter)
    {
        var value = interpreter.EvalOperand(instr.Operands[0]);
        var level = interpreter.ParseEnumOperand<NodePrint.EPrintLevel>(instr.Operands[1]);

        switch (level)
        {
            case NodePrint.EPrintLevel.Debug:
                Log.Debug("[BP] {Value}", value);
                break;
            case NodePrint.EPrintLevel.Info:
                Log.Information("[BP] {Value}", value);
                break;
            case NodePrint.EPrintLevel.Warning:
                Log.Warning("[BP] {Value}", value);
                break;
            case NodePrint.EPrintLevel.Error:
                Log.Error("[BP] {Value}", value);
                break;
            default:
                throw new InvalidOperationException($"Unknown print level: {level}");
        }
    }

    public EGraphOperandKind[] Signature { get; }
        = [EGraphOperandKind.Register | EGraphOperandKind.Literal, EGraphOperandKind.Register | EGraphOperandKind.Enum];
}