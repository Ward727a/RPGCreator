namespace RPGCreator.SDK.Parser.PrattFormula;

public abstract record PrattExpr
{
    public record Number(double Value) : PrattExpr;
    public record Variable(string Name) : PrattExpr;
    public record Prefix(EPrattTokenKind Operator, PrattExpr Right) : PrattExpr;
    public record Postfix(PrattExpr Left, EPrattTokenKind Operator) : PrattExpr;
    public record Infix (PrattExpr Left, EPrattTokenKind Operator, PrattExpr Right) : PrattExpr;
    public record Call(PrattExpr Callee, IReadOnlyList<PrattExpr> Arguments) : PrattExpr;
    public record Index(PrattExpr Target, PrattExpr Indexer) : PrattExpr;
    public record Member(PrattExpr Target, string MemberName) : PrattExpr;
    public record Ternary(PrattExpr Condition, PrattExpr TrueExpr, PrattExpr FalseExpr) : PrattExpr;
}