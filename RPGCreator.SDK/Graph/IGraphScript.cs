namespace RPGCreator.SDK.Graph;

public interface IGraphScript
{
    string DocumentPath { get; }
    IList<GraphLabeledInstr> GetInstructions();
    IReadOnlyDictionary<string, (System.Type, object)> GetVariables();
}