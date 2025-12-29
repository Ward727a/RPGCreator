using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

public sealed class PrattCompiledFormula(PrattExpr ast) : IPrattFormula
{
    private readonly PrattExpr _ast = ast ?? throw new ArgumentNullException(nameof(ast)); // The abstract syntax tree representing the formula

    public double Eval(PrattEvaluationEnvironment? env = null)
    {
        env ??= new PrattEvaluationEnvironment();
        return PrattInterpreter.Evaluate(_ast, env);
    }

    public PrattExpr GetAst() => _ast;
}