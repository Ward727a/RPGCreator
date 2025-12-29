using RPGCreator.SDK.Graph;
using Serilog;

namespace RPGCreator.Core.Parser.Graph;

public class GraphDocumentCompiled(List<GraphLabeledInstr> instructions, Dictionary<string, (System.Type, object)>? variables = null) : IGraphScript
{
    public string DocumentPath { get; set; } = string.Empty;
    List<GraphLabeledInstr> Instructions { get; } = instructions;
    public IList<GraphLabeledInstr> GetInstructions() => Instructions.ToList();
    
    Dictionary<string, (System.Type, object)> Variables { get; } = variables ?? new();
    public IReadOnlyDictionary<string, (System.Type, object)> GetVariables() => Variables;

    public bool Run(GraphEvalEnvironment? env = null)
    {
        if(env == null)
        {
            env = new GraphEvalEnvironment();
            
            foreach(var variable in Variables)
            {
                env.SetVariable(variable.Key, variable.Value.Item2);
            }
        }
        
        var interpreter = new GraphInterpreter(GetInstructions(), env);
        
        try
        {
            interpreter.Run();
            return true;
        } catch (Exception ex)
        {
            Log.Error(ex, "Graph execution error: {ex.Message}", ex.Message);
            return false;
        }
    }
}