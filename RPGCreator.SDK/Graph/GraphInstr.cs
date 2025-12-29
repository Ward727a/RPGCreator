namespace RPGCreator.SDK.Graph;

public readonly struct GraphOperands
{
    public EGraphOperandKind Kind { get; }
    public string Text { get; }
    public object Value { get; }

    public GraphOperands(EGraphOperandKind kind, string text, object value)
    {
        Kind = kind;
        Text = text;
        Value = value;
    }
}

public sealed record GraphLabeledInstr(string Label, List<GraphInstr> Instrs);

public sealed record GraphInstr(EGraphOpCode OpCode, params GraphOperands[] Operands);
// public sealed record GraphInstr(string Op, params string[] Args);
public static class GraphIR
{
    public static GraphInstr Op(EGraphOpCode opCode, params GraphOperands[] operands) => new(opCode, operands);
    public static GraphOperands Operands(EGraphOperandKind kind, string text) => new(kind, text, text);
    public static GraphOperands Operands(EGraphOperandKind kind, string text, object value) => new(kind, text, value);
    public static GraphLabeledInstr Label(string label, List<GraphInstr> instrs) => new(label, instrs);
    public static GraphLabeledInstr Label(string label, params GraphInstr[] instrs) => new(label, instrs.ToList());
}