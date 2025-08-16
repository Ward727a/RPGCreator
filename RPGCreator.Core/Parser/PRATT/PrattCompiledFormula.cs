namespace RPGCreator.Core.Parser.PRATT;

public sealed class PrattCompiledFormula(PrattExpr ast)
{
    private readonly PrattExpr _ast = ast ?? throw new ArgumentNullException(nameof(ast)); // The abstract syntax tree representing the formula

    public double Eval(PrattEvaluationEnvironment? env = null)
    {
        env ??= new PrattEvaluationEnvironment();
        return PrattInterpreter.Evaluate(_ast, env);
    }
}