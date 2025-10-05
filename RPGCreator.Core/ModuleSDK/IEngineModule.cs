namespace RPGCreator.Core.ModuleSDK;

public interface IEngineModule
{
    public Version TargetEngineVersion { get; }
    
    public string Name { get; }
    public string Version { get; }
    public string Author { get; }
    public string Description { get; }
    public void Initialize();
}