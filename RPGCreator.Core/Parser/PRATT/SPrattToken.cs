using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

public struct SPrattToken
{
    public readonly EPrattTokenKind Kind;
    public readonly string Lexeme; // The actual text of the token
    public readonly int Start; // Start position in the source code
    public readonly int Length; // Length of the token in the source code
    public SPrattToken(EPrattTokenKind kind, string lexeme, int start, int length)
    {
        Kind = kind;
        Lexeme = lexeme;
        Start = start;
        Length = length;
    }
}