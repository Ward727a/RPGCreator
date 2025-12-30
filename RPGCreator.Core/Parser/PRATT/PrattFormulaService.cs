using RPGCreator.SDK.Logging;
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

    public bool TryCompile(string formulaText, out IPrattFormula? formula)
    {
        
        if(string.IsNullOrWhiteSpace(formulaText))
        {
            formula = null;
            Logger.Warning("Cannot compile empty formula.");
            return false;
        }
        
        try
        {
            var compiler = new PrattCompiler();
            formula = compiler.Compile(formulaText);
            return true;
        }
        catch(Exception ex)
        {
            formula = null;
            Logger.Warning("Failed to compile formula: {formulaText} error: {errorMsg}", formulaText, ex.Message);
            return false;
        }
    }
}