using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

public static class PrattInterpreter
{
    public static double Evaluate(PrattExpr expr, PrattEvaluationEnvironment env)
    {
        switch (expr)
        {
            case PrattExpr.Number number: return number.Value;
            case PrattExpr.Variable variable: return GetVar(variable.Name, env);
            case PrattExpr.Prefix prefix:
            {
                var r = Evaluate(prefix.Right, env);
                return prefix.Operator switch
                {
                    EPrattTokenKind.Plus => r,
                    EPrattTokenKind.Minus => -r,
                    EPrattTokenKind.Bang => env.ToNum(!env.IsTrue(r)),
                    _ => throw new Exception($"Unexpected prefix operator {prefix.Operator}.")
                };
            }
            case PrattExpr.Postfix postfix:
            {
                var left = Evaluate(postfix.Left, env);
                return postfix.Operator switch
                {
                    EPrattTokenKind.Bang => Factorial(left),
                    EPrattTokenKind.Percent => left / 100.0,
                    _ => throw new Exception($"Unexpected postfix operator {postfix.Operator}.")
                };
            }
            
            case PrattExpr.Infix infix:
                return EvaluateInfix(infix.Operator, infix.Left, infix.Right, env);

            case PrattExpr.Call call:
            {
                var name = (call.Callee as PrattExpr.Variable)?.Name
                    ?? throw new Exception("Only simple function names supported in calls.");

                if (!env.Functions.TryGetValue(name, out var fun)) 
                    throw new Exception($"Function '{name}' not found in environment.");
                
                var args = new double[call.Arguments.Count];
                for (int i = 0; i < call.Arguments.Count; i++)
                    args[i] = Evaluate(call.Arguments[i], env);
                return fun(args);
            }
            
            case PrattExpr.Index index:
                throw new NotSupportedException("Indexing is not supported in the interpreter (for now).");
            
            case PrattExpr.Member member:
                throw new NotSupportedException("Member access is not supported in the interpreter (for now).");

            case PrattExpr.Ternary ternary:
            {
                var condition = Evaluate(ternary.Condition, env);
                if (env.IsTrue(condition)) return Evaluate(ternary.TrueExpr, env);
                else return Evaluate(ternary.FalseExpr, env);
            }
            default:
                throw new NotSupportedException(expr.GetType().Name + " is not supported in the interpreter.");
        }
    }

    private static double GetVar(string name, PrattEvaluationEnvironment env)
    {
        return env.Variables.TryGetValue(name, out var value) ? value : throw new Exception($"Variable '{name}' not found in environment.");
    }

    private static double Factorial(double value)
    {
        if(value < 0) 
            throw new ArgumentException("Factorial is not defined for negative numbers.");

        var roundedValue = (long)Math.Round(value);
        if(Math.Abs(roundedValue - value) > 1e-9) 
            throw new ArgumentException("Factorial is only defined for integers.");
        
        long accumulator = 1;
        for (long i = 2; i <= roundedValue; i++)
            accumulator *= i;
        return accumulator;
    }

    private static double EvaluateInfix(EPrattTokenKind operatorKind, PrattExpr leftExpr, PrattExpr rightExpr,
        PrattEvaluationEnvironment env)
    {
        switch (operatorKind)
        {
            case EPrattTokenKind.AndAnd:
            {
                var leftValue = Evaluate(leftExpr, env);
                if(!env.IsTrue(leftValue)) return env.ToNum(false);
                var rightValue = Evaluate(rightExpr, env);
                return env.ToNum(env.IsTrue(rightValue));
            }
            case EPrattTokenKind.OrOr:
            {
                var leftValue = Evaluate(leftExpr, env);
                if(env.IsTrue(leftValue)) return env.ToNum(true);
                var rightValue = Evaluate(rightExpr, env);
                return env.ToNum(env.IsTrue(rightValue));
            }

            default:
            {
                var leftValue = Evaluate(leftExpr, env);
                var rightValue = Evaluate(rightExpr, env);
                return operatorKind switch
                {
                    EPrattTokenKind.Plus => leftValue + rightValue,
                    EPrattTokenKind.Minus => leftValue - rightValue,
                    EPrattTokenKind.Multiply => leftValue * rightValue,
                    EPrattTokenKind.Divide => (rightValue == 0)
                        ? throw new DivideByZeroException()
                        : leftValue / rightValue,
                    EPrattTokenKind.Percent => leftValue % rightValue,
                    EPrattTokenKind.Caret => Math.Pow(leftValue, rightValue),

                    EPrattTokenKind.EqEq => leftValue == rightValue ? 1 : 0,
                    EPrattTokenKind.NotEq => leftValue != rightValue ? 1 : 0,
                    EPrattTokenKind.LessThan => leftValue < rightValue ? 1 : 0,
                    EPrattTokenKind.GreaterThan => leftValue > rightValue ? 1 : 0,
                    EPrattTokenKind.LessThanEq => leftValue <= rightValue ? 1 : 0,
                    EPrattTokenKind.GreaterThanEq => leftValue >= rightValue ? 1 : 0,
                    _ => throw new Exception($"Unexpected infix operator {operatorKind}.")
                };
            }
        }
    }
}