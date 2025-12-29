using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

public class PrattFormulaService : IPrattFormulaService
{
    public double Evaluate(IPrattFormula formula, IDictionary<string, double> variables)
    {
        if (formula == null) throw new ArgumentNullException(nameof(formula));
        if (variables == null) throw new ArgumentNullException(nameof(variables));

        var env = new PrattEvaluationEnvironment() { Variables = variables.AsReadOnly() };

        return PrattInterpreter.Evaluate(formula.GetAst(), env);
    }
}