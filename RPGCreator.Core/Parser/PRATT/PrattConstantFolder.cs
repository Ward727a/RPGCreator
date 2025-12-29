using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

/// <summary>
/// This class is responsible for constant folding in the PRATT parser.<br/>
/// Constant folding is an optimization technique that simplifies expressions at compile time.<br/>
/// It evaluates constant expressions and replaces them with their computed values.<br/>
/// Constant expressions are those that can be fully evaluated at compile time, such as numeric literals and simple arithmetic operations.<br/>
/// This can help improve performance by reducing the number of calculations needed at runtime.
/// </summary>
public static class PrattConstantFolder
{
    public static PrattExpr Fold(PrattExpr expr, IReadOnlyDictionary<string, Func<double[], double>>? pureFuncs = null)
    {
        pureFuncs ??= PrattBasicFunctions.Default; // Default to built-in functions if none provided
        
        switch(expr)
        {
            case PrattExpr.Number or PrattExpr.Variable:
                // Numbers and variables are already in their simplest form
                return expr;
            case PrattExpr.Prefix prefix:
            {
                var foldedRight = Fold(prefix.Right, pureFuncs);
                if (foldedRight is PrattExpr.Number number)
                    return new PrattExpr.Number(EvalPrefixConstant(prefix.Operator, number.Value));
                return prefix with { Right = foldedRight };
            }

            case PrattExpr.Postfix postfix:
            {
                var foldedLeft = Fold(postfix.Left, pureFuncs);
                if (foldedLeft is PrattExpr.Number leftNumber)
                {
                    return new PrattExpr.Number(EvalPostfixConstant(leftNumber.Value, postfix.Operator));
                }
                return postfix with { Left = foldedLeft };
            }

            case PrattExpr.Infix infix:
            {
                var foldedLeft = Fold(infix.Left, pureFuncs);
                var foldedRight = Fold(infix.Right, pureFuncs);

                if (foldedLeft is PrattExpr.Number leftNumber && foldedRight is PrattExpr.Number rightNumber)
                {
                    return new PrattExpr.Number(EvalInfixConstant(infix.Operator, leftNumber.Value, rightNumber.Value));
                }

                if (infix.Operator == EPrattTokenKind.Plus)
                {
                    if (foldedRight is PrattExpr.Number { Value: 0 }) return foldedLeft;
                    if (foldedLeft is PrattExpr.Number { Value: 0 }) return foldedRight;
                }

                if (infix.Operator == EPrattTokenKind.Multiply)
                {
                    if(foldedRight is PrattExpr.Number { Value: 1 }) return foldedLeft;
                    if(foldedLeft is PrattExpr.Number { Value: 1 }) return foldedRight;
                    if(foldedRight is PrattExpr.Number { Value: 0 } || foldedLeft is PrattExpr.Number { Value: 0 })
                        return new PrattExpr.Number(0.0); // Multiplying by zero
                }

                return infix with { Left = foldedLeft, Right = foldedRight };
            }

            case PrattExpr.Ternary ternary:
            {
                var condition = Fold(ternary.Condition, pureFuncs);
                var trueExpr = Fold(ternary.TrueExpr, pureFuncs);
                var falseExpr = Fold(ternary.FalseExpr, pureFuncs);
                
                if(condition is PrattExpr.Number conditionNumber)
                    return (conditionNumber.Value != 0.0) ? trueExpr : falseExpr;
                return ternary with {Condition = condition, TrueExpr = trueExpr, FalseExpr = falseExpr};
            }

            case PrattExpr.Call call:
            {
                var args2 = call.Arguments.Select(arg => Fold(arg, pureFuncs)).ToArray();

                if (call.Callee is PrattExpr.Variable v && pureFuncs.TryGetValue(v.Name, out var func) &&
                    args2.All(x => x is PrattExpr.Number))
                {
                    var arr = args2.Cast<PrattExpr.Number>().Select(n => n.Value).ToArray();
                    return new PrattExpr.Number(func(arr));
                }
                
                return call with { Arguments = args2 };
            }
            default:
                return expr;
        }
    }

    private static double EvalPrefixConstant(EPrattTokenKind @operator, double value)
    {
        return @operator switch
        {
            EPrattTokenKind.Plus => value, // Unary plus does nothing
            EPrattTokenKind.Minus => -value, // Unary minus negates the value
            EPrattTokenKind.Bang => value == 0 ? 1.0 : 0.0, // Logical NOT: 0 becomes 1, non-zero becomes 0
            _ => throw new NotSupportedException($"Unsupported prefix operator: {@operator}")
        };
    }

    private static double EvalPostfixConstant(double value, EPrattTokenKind @operator)
    {
        return @operator switch
        {
            EPrattTokenKind.Percent => value / 100.0,
            _ => throw new NotSupportedException($"Unsupported postfix operator: {@operator}")
        };
    }
    
    private static double EvalInfixConstant(EPrattTokenKind @operator, double left, double right)
    {
        return @operator switch
        {
            EPrattTokenKind.Plus  => left + right,
            EPrattTokenKind.Minus => left - right,
            EPrattTokenKind.Multiply  => left * right,
            EPrattTokenKind.Divide => (right == 0) ? throw new DivideByZeroException() : left / right,
            EPrattTokenKind.Caret => Math.Pow(left, right),

            EPrattTokenKind.EqEq  => left == right ? 1 : 0,
            EPrattTokenKind.NotEq => left != right ? 1 : 0,
            EPrattTokenKind.LessThan => left <  right ? 1 : 0,
            EPrattTokenKind.LessThanEq => left <= right ? 1 : 0,
            EPrattTokenKind.GreaterThan => left >  right ? 1 : 0,
            EPrattTokenKind.GreaterThanEq=> left >= right ? 1 : 0,
            _ => throw new NotSupportedException($"Unsupported infix operator: {@operator}")
        };
    }
    
}