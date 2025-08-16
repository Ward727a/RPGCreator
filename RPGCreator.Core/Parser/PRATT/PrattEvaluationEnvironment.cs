namespace RPGCreator.Core.Parser.PRATT;

public sealed class PrattEvaluationEnvironment
{
    public IReadOnlyDictionary<string, double> Variables { get; init; } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, Func<double[], double>> Functions { get; init; } = PrattBasicFunctions.Default;
    public Func<double, bool> IsTrue { get; init; } = v => v != 0.0;
    public Func<bool, double> ToNum { get; init; } = b => b ? 1.0 : 0.0;
}