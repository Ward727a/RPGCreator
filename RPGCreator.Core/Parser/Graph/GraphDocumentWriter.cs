using System.Text;
using RPGCreator.SDK.Graph;

namespace RPGCreator.Core.Parser.Graph;

public sealed class GraphDocumentWriter
{
    
    private IReadOnlyList<GraphLabeledInstr> _program {get; init; }

    public GraphDocumentWriter(List<GraphLabeledInstr> program)
    {
        _program = program.AsReadOnly();
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        foreach (var block in _program)
        {
            if (!string.IsNullOrEmpty(block.Label))
            {
                sb.AppendLine(block.Label);
            }
            
            foreach(GraphInstr inst in block.Instrs)
            {
                sb.Append(inst.OpCode.ToString().ToLowerInvariant());
                if (inst.Operands.Length > 0)
                {
                    sb.Append(' ');
                    sb.Append(string.Join(" ", inst.Operands.Select(o => $"{(o.Text.StartsWith("rx") ? o.Text : o.Text.Replace("_", "\\_").Replace(" ", "_"))}")));
                }
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }
    
}