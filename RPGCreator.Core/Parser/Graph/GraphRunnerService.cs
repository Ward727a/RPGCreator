using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Logging;

namespace RPGCreator.Core.Parser.Graph;

internal class GraphRunnerService : IGraphRunnerService
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
}