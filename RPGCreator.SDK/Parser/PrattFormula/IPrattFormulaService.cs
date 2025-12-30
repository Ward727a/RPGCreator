namespace RPGCreator.SDK.Parser.PrattFormula;

public interface IPrattFormulaService
{
    double Evaluate(IPrattFormula formula, IDictionary<string, double> variables);
    bool TryCompile(string formulaText, out IPrattFormula? formula);
}