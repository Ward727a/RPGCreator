namespace RPGCreator.SDK.Graph;

public interface IGraphScript
{
    string DocumentPath { get; }
    IList<GraphLabeledInstr> GetInstructions();
    IReadOnlyDictionary<string, (Type, object)> GetVariables();
}