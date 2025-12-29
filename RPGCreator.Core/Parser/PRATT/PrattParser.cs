using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

public sealed class PrattParser
{
    private readonly List<SPrattToken> _tokens;
    private int _currentIndex;
    private readonly PrattTable _table;
    
    public PrattParser(List<SPrattToken> tokens, PrattTable table)
    {
        _tokens = tokens;
        _currentIndex = 0;
        _table = table;
    }

    private SPrattToken CurrentToken => _currentIndex < _tokens.Count ? _tokens[_currentIndex] : _tokens[^1];

    private SPrattToken NextToken()
    {
        var token = CurrentToken;
        _currentIndex++;
        return token;
    }

    private bool Match(EPrattTokenKind kind)
    {
        if (CurrentToken.Kind == kind)
        {
            _currentIndex++;
            return true;
        }

        return false;
    }

    private SPrattToken Expect(EPrattTokenKind kind, string errorMessage)
    {
        if(CurrentToken.Kind != kind)
            throw new Exception($"{errorMessage} at {CurrentToken.Start}[{CurrentToken.Length}]:{CurrentToken.Lexeme} | Expected {kind} but found {CurrentToken.Kind}.");
        return NextToken();
    }

    public PrattExpr ParseExpression(int minBindingPower = 0)
    {
        PrattExpr left = CurrentToken.Kind switch
        {
            EPrattTokenKind.Number => new PrattExpr.Number(double.Parse(NextToken().Lexeme,
                System.Globalization.CultureInfo.InvariantCulture)),
            EPrattTokenKind.Identifier => ParseIdentifierOrCallOrMemberNud(),
            EPrattTokenKind.LParen => ParseParenGroup(), // Parse a parenthesis group
            EPrattTokenKind.Plus => ParsePrefix(),
            EPrattTokenKind.Minus => ParsePrefix(),
            EPrattTokenKind.Bang => ParsePrefix(),
            _ => throw new Exception(
                $"Unexpected token {CurrentToken.Kind} at {CurrentToken.Start}[{CurrentToken.Length}]:{CurrentToken.Lexeme}.")
        };

        while (true)
        {
            var kind = CurrentToken.Kind;

            if (_table.TryGetPostfix(kind, out var pfLeftBindingPower) && pfLeftBindingPower >= minBindingPower)
            {
                var operatorKind = NextToken().Kind;
                left = new PrattExpr.Postfix(left, operatorKind);
                continue;
            }

            if (kind == EPrattTokenKind.LParen && 100 >= minBindingPower)
            {
                left = ParseCall(left);
                continue;
            }

            if (kind == EPrattTokenKind.LBracket && 100 >= minBindingPower)
            {
                NextToken();
                var index = ParseExpression(0);
                Expect(EPrattTokenKind.RBracket, "Missing closing bracket ']'");
                left = new PrattExpr.Index(left, index);
                continue;
            }

            if (kind == EPrattTokenKind.Dot && 100 >= minBindingPower)
            {
                NextToken();
                var identifier = Expect(EPrattTokenKind.Identifier, "Expected identifier after '.'");
                left = new PrattExpr.Member(left, identifier.Lexeme);
                continue;
            }

            if (kind == EPrattTokenKind.Question)
            {
                if (30 < minBindingPower) break;
                NextToken();
                var whenTrue = ParseExpression(0);
                Expect(EPrattTokenKind.Colon, "Expected ':' after '?'");
                var whenFalse = ParseExpression(29);
                left = new PrattExpr.Ternary(left, whenTrue, whenFalse);
                continue;
            }

            if (_table.TryGetInfix(kind, out var BindingPowers))
            {
                if(BindingPowers.lbp < minBindingPower) break;
                var operatorKind = NextToken().Kind;
                var right = ParseExpression(BindingPowers.rbp);
                left = new PrattExpr.Infix(left, operatorKind, right);
                continue;
            }

            break;
        }

        return left;
    }

    private PrattExpr ParsePrefix()
    {
        var operatorKind = NextToken().Kind;
        if(!_table.TryGetPrefix(operatorKind, out var rightBindingPower)) throw new Exception($"No prefix binding power for {operatorKind} at {CurrentToken.Start}[{CurrentToken.Length}]:{CurrentToken.Lexeme}.");
        var right = ParseExpression(rightBindingPower);
        return new PrattExpr.Prefix(operatorKind, right);
    }

    private PrattExpr ParseParenGroup()
    {
        NextToken();
        var expr = ParseExpression(0);
        Expect(EPrattTokenKind.RParen, "Missing closing parenthesis ')'");
        return expr;
    }

    private PrattExpr ParseCall(PrattExpr callee)
    {
        Expect(EPrattTokenKind.LParen, "Missing opening parenthesis '('");
        var arguments = new List<PrattExpr>();
        if (CurrentToken.Kind != EPrattTokenKind.RParen)
        {
            do
            {
                arguments.Add(ParseExpression(0));
            } while (Match(EPrattTokenKind.Comma));
        }
        Expect(EPrattTokenKind.RParen, "Missing closing parenthesis ')'");
        return new PrattExpr.Call(callee, arguments);
    }

    private PrattExpr ParseIdentifierOrCallOrMemberNud()
    {
        var identifier = NextToken();
        return new PrattExpr.Variable(identifier.Lexeme);
    }
}