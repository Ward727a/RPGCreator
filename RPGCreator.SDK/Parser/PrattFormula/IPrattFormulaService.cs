namespace RPGCreator.SDK.Parser.PrattFormula;

public interface IPrattFormulaService
{
    double Evaluate(IPrattFormula formula, IDictionary<string, double> variables);
}