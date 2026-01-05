using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Logging;

namespace RPGCreator.Core.Parser.Graph;

internal class GraphService : IGraphService
{
    private Stack<GraphInterpreter> _interpreters = new Stack<GraphInterpreter>(100);
    public bool Run(IGraphScript script, IGraphEnv env)
    {
        if (env is not GraphEvalEnvironment evalEnvironment)
        {
            Logger.Error("GraphRunnerService.Run: Invalid environment type. Expected GraphEvalEnvironment. Got " + env.GetType().Name);
            return false;
        }

        var instructions = script.GetInstructions();
        
        if(_interpreters.TryPop(out var graphInterpreter))
        {
            graphInterpreter.Reset(instructions, evalEnvironment);
        }
        else
        {
            graphInterpreter = new GraphInterpreter(instructions, evalEnvironment);
        }

        try
        {
            graphInterpreter.Run();
            return true;
        }
        catch (Exception ex)
        {
            Logger.Error("GraphRunnerService.Run: Exception during graph execution: " + ex.Message + "\n" +
                         ex.StackTrace + "\n At: Block(" + evalEnvironment.CurrentBlock + ") - Instr(" +
                         evalEnvironment.CurrentInstruction + ")");
            return false;
        }
        finally
        {
            _interpreters.Push(graphInterpreter);
        }
    }

    public IGraphEnv CreateEnvironment()
    {
        return new GraphEvalEnvironment();
    }

    private readonly Dictionary<string, IGraphScript> _scriptCache = new Dictionary<string, IGraphScript>(StringComparer.OrdinalIgnoreCase);

    public bool TryLoadScript(string scriptPath, [NotNullWhen(true)] out IGraphScript? script)
    {

        if (_scriptCache.TryGetValue(scriptPath, out var cachedScript))
        {
            script = cachedScript;
            return true;
        }
        
        script = null;

        if (!File.Exists(scriptPath)) return false;

        try
        {
            var json = File.ReadAllText(scriptPath);

            EngineCore.Instance.Serializer.Deserialize<GraphDocument>(json, out var graphDocument);

            if (graphDocument != null)
            {
                script = GraphDocumentCompiler.Compile(graphDocument);
                return true;
            }
        }
        catch
        {
            Logger.Error("GraphRunnerService.TryLoadScript: Failed to load or deserialize graph script at path " + scriptPath);
        }
        return false;
    }

    public bool TryLoadDocument(string scriptPath, [NotNullWhen(true)] out GraphDocument? document)
    {
        document = null;

        if (!File.Exists(scriptPath)) return false;

        try
        {
            var json = File.ReadAllText(scriptPath);

            EngineCore.Instance.Serializer.Deserialize<GraphDocument>(json, out var graphDocument);

            if (graphDocument != null)
            {
                document = graphDocument;
                return true;
            }
        }
        catch
        {
            Logger.Error("GraphRunnerService.TryLoadDocument: Failed to load or deserialize graph document at path " + scriptPath);
        }
        return false;
    }

    public void InvalidateCache(string scriptPath)
    {
        if (_scriptCache.Remove(scriptPath))
        {
            Logger.Info("GraphRunnerService.InvalidateCache: " + scriptPath);
        }
    }

    public void ClearCache()
    {
        _scriptCache.Clear();
        Logger.Info("GraphRunnerService.ClearCache: All cached scripts have been cleared.");
    }

    public IGraphScript Compile(GraphDocument document)
    {
        return GraphDocumentCompiler.Compile(document);
    }
}