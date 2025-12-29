namespace RPGCreator.SDK.Graph;

[Flags]
public enum EGraphOperandKind
{
    None = 0,
    Register = 1<<0,
    Literal = 1<<1,
    LiteralString = 1<<2,
    Path = 1<<3,
    Label = 1<<4,
    Enum = 1<<5,
    LiteralNumber = 1<<6,
}