using System.Diagnostics.CodeAnalysis;

namespace RPGCreator.SDK.Graph;

public interface IGraphService
{
    bool Run(IGraphScript script, IGraphEnv env);
    IGraphEnv CreateEnvironment();
    bool TryLoadScript(string scriptPath, [NotNullWhen(true)]out IGraphScript? script);

    public void InvalidateCache(string scriptPath);
    public void ClearCache();
}