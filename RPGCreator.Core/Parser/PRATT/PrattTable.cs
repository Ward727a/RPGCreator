using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

public sealed class PrattTable
{
    private readonly Dictionary<EPrattTokenKind, (int lbp, int rbp)> _infix = new();
    private readonly Dictionary<EPrattTokenKind, int> _postfix = new(); // left binding power for postfix operators
    private readonly Dictionary<EPrattTokenKind, int> _prefix = new(); // right binding power for prefix operators

    public void InfixLeft(EPrattTokenKind kind, int bindingPower) => _infix[kind] = (bindingPower, bindingPower);
    public void InfixRight(EPrattTokenKind kind, int bindingPower) => _infix[kind] = (bindingPower, bindingPower - 1);
    public void Postfix(EPrattTokenKind kind, int bindingPower) => _postfix[kind] = bindingPower;
    public void Prefix(EPrattTokenKind kind, int bindingPower) => _prefix[kind] = bindingPower;
    
    public bool TryGetInfix(EPrattTokenKind kind, out (int lbp, int rbp) bindingPower)
    {
        return _infix.TryGetValue(kind, out bindingPower);
    }
    
    public bool TryGetPostfix(EPrattTokenKind kind, out int bindingPower)
    {
        return _postfix.TryGetValue(kind, out bindingPower);
    }

    public bool TryGetPrefix(EPrattTokenKind kind, out int bindingPower)
    {
        return _prefix.TryGetValue(kind, out bindingPower);
    }
    
    
    public static PrattTable DefaultPrattTable()
    {
        var prattTable = new PrattTable();
        
        // Postfix // Callee
        prattTable.Postfix(EPrattTokenKind.Bang, 90); // Postfix operator '!'
        prattTable.Postfix(EPrattTokenKind.Percent, 90); // Postfix operator '%'
        // Prefixes
        prattTable.Prefix(EPrattTokenKind.Plus, 80); // Unary plus '+'
        prattTable.Prefix(EPrattTokenKind.Minus, 80); // Unary minus '-'
        prattTable.Prefix(EPrattTokenKind.Bang, 80); // Logical NOT '!'
        // Infixes
        prattTable.InfixRight(EPrattTokenKind.Caret, 70); // Exponentiation '^' e.g. 2^3
        prattTable.InfixLeft(EPrattTokenKind.Multiply, 60); // Multiplication '*'
        prattTable.InfixLeft(EPrattTokenKind.Divide, 60); // Division '/'
        prattTable.InfixLeft(EPrattTokenKind.Plus, 50); // Addition '+'
        prattTable.InfixLeft(EPrattTokenKind.Minus, 50); // Subtraction '-'
        prattTable.InfixLeft(EPrattTokenKind.LessThan, 40); // Less than '<'
        prattTable.InfixLeft(EPrattTokenKind.GreaterThan, 40); // Greater than '>'
        prattTable.InfixLeft(EPrattTokenKind.LessThanEq, 40); // Less than or equal to '<='
        prattTable.InfixLeft(EPrattTokenKind.GreaterThanEq, 40); // Greater than or equal to '>='
        prattTable.InfixLeft(EPrattTokenKind.EqEq, 35);
        prattTable.InfixLeft(EPrattTokenKind.NotEq, 35); // Equality '=='
        prattTable.InfixLeft(EPrattTokenKind.AndAnd, 34); // Logical AND '&&'
        prattTable.InfixLeft(EPrattTokenKind.OrOr, 33); // Logical OR '||'

        return prattTable;
    }
}