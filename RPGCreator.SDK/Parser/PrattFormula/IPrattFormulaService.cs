namespace RPGCreator.SDK.Parser.PrattFormula;

public interface IPrattFormulaService : IService
{
    double Evaluate(IPrattFormula formula, IDictionary<string, double> variables);
    bool TryCompile(string formulaText, out IPrattFormula? formula);
}