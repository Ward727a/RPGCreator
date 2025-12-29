namespace RPGCreator.SDK.Graph;

public interface IGraphRunnerService
{
    bool Run(IGraphScript script, IGraphEnv env);
}