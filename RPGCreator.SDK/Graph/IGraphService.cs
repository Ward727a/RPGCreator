using System.Diagnostics.CodeAnalysis;

namespace RPGCreator.SDK.Graph;

public interface IGraphService : IService
{
    bool Run(IGraphScript script, IGraphEnv env);
    IGraphEnv CreateEnvironment();
    bool TryLoadScript(string scriptPath, [NotNullWhen(true)]out IGraphScript? script);
    bool TryLoadDocument(string scriptPath, [NotNullWhen(true)]out GraphDocument? document);

    public void InvalidateCache(string scriptPath);
    public void ClearCache();
    
    IGraphScript Compile(GraphDocument document);
}