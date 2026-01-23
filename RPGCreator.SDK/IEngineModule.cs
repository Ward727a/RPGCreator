using RPGCreator.SDK.Types;

namespace RPGCreator.SDK;

public interface IEngineModule : IEngineModuleVersion
{
    
    public string Name { get; }
    public string Version { get; }
    public string Author { get; }
    public string Description { get; }
    public void Initialize();
    public void Shutdown();
}

/// <summary>
/// This interface is separated from the main IEngineModule so we can change the IEngineModule,<br/>
/// Without breaking compatibility for old modules so we can at least check the version first.<br/>
/// <br/>
/// NOTE: For this to work, this interface MUST NOT be changed ever!!
/// </summary>
public interface IEngineModuleVersion
{
    /// <summary>
    /// The target engine version this module is built for.<br/>
    /// This allows the engine to check if the module is compatible with the current engine version.<br/>
    /// If the engine version is lower than the target version, the module might not work properly.
    /// </summary>
    public Version TargetEngineVersion { get; }
    
    /// <summary>
    /// The URN of the module.<br/>
    /// This allows to uniquely identify the module in the engine.<br/>
    /// It is recommended to use the format like: "[Author]:Module@[ModuleName]".<br/>
    /// Like: "Ward727:Module@RPGCreatorCore"
    /// </summary>
    public URN ModuleUrn { get; }
}