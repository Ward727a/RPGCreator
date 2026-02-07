using System.Diagnostics;
using System.Reflection;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;


namespace RPGCreator.SDK.Modules;

/// <summary>
/// A security token used to ensure that only the engine can call certain methods on modules.<br/>
/// This token is provided by the engine when calling the Shutdown method on modules.<br/>
/// This prevents external code from shutting down modules directly, ensuring that only the engine has control over module lifecycle.
/// <br/>
/// This class has an internal constructor, so it can only be instantiated by the engine itself.
/// </summary>
public sealed class EngineSecurityToken 
{
    internal EngineSecurityToken() { }
}

/// <summary>
/// A module candidate represent a module that can be started by the engine.<br/>
/// It contains all the metadata about the module, as well as the type of the module itself.<br/>
/// This struct is used by the module manager to keep track of all available modules, and to start them when needed.
/// <br/>
/// This struct implements the IEngineModuleInfo interface, so it can be used to provide module information without exposing the full module type.
/// </summary>
public struct ModuleCandidate : IEngineModuleInfo
{
    public Version TargetEngineVersion { get; }
    public URN ModuleUrn { get; }
    public string Name { get; }
    public string Version { get; }
    public string Author { get; }
    public string Description { get; }
    public URN[] Dependencies { get; }
    public URN[] Incompatibilities { get; }
    internal Type? ModuleType { get; }

    public ModuleCandidate(ModuleManifestAttribute manifest, Type type)
    {
        ModuleUrn = manifest.Urn;
        Name = manifest.Name;
        Version = manifest.Version;
        Author = manifest.Author;
        Description = manifest.Description;
        TargetEngineVersion = System.Version.Parse(manifest.TargetEngineVersion);
        Dependencies = (manifest?.Dependencies ?? []).Select(urnStr => new URN(urnStr)).ToArray();
        Incompatibilities = (manifest?.Incompatibilities ?? []).Select(urnStr => new URN(urnStr)).ToArray();;
        ModuleType = type;
    }

    public ModuleCandidate(IEngineModuleInfo moduleInfo)
    {
        ModuleUrn = moduleInfo.ModuleUrn;
        Name = moduleInfo.Name;
        Version = moduleInfo.Version;
        Author = moduleInfo.Author;
        Description = moduleInfo.Description;
        TargetEngineVersion = moduleInfo.TargetEngineVersion;
        Dependencies = moduleInfo.Dependencies;
        Incompatibilities = moduleInfo.Incompatibilities;
        ModuleType = null;
    }
}

public abstract class BaseModule : IEngineModuleInfo {
    private ModuleManifestAttribute? _manifest => 
        GetType().Assembly.GetCustomAttribute<ModuleManifestAttribute>();
    public Version TargetEngineVersion => 
        System.Version.Parse(_manifest?.TargetEngineVersion ?? "1.0.0");
    public URN ModuleUrn => 
        new URN(_manifest?.Urn ?? "unknown://module/unknown");
    public string Name => 
        _manifest?.Name ?? "Unknown Module";
     public string Version => 
        _manifest?.Version ?? "1.0.0";
    public string Author => 
        _manifest?.Author ?? "Unknown Author";
    public string Description => 
        _manifest?.Description ?? "No description provided.";
    public URN[] Dependencies => 
        (_manifest?.Dependencies ?? []).Select(urnStr => new URN(urnStr)).ToArray();
    public URN[] Incompatibilities => 
        (_manifest?.Incompatibilities ?? []).Select(urnStr => new URN(urnStr)).ToArray();

    public bool IsFirstTime = false;
    
    /// <summary>
    /// Check if this is the first time the module is initialized by looking for a specific file in the module folder.<br/>
    /// This can be used to run some initialization code only the first time the module is initialized, such as creating default assets or folders.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public bool IsFirstTimeInitialization()
    {
        var moduleFolder = Path.GetDirectoryName(this.GetType().Assembly.Location);
        
        if (Directory.Exists(moduleFolder))
        {
            var firstTimeFilePath = Path.Combine(moduleFolder, ".initialized");
            if (File.Exists(firstTimeFilePath))
            {
                return false;
            }

            File.Create(firstTimeFilePath).Close();
            return true;
        }

        throw new DirectoryNotFoundException($"Module folder not found: {moduleFolder}");
    }
    
    protected abstract void OnInitialize();
    protected abstract void OnShutdown();

    internal void Initialize(EngineSecurityToken token)
    {
        StackTrace stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
        if (token == null)
        {
            throw new UnauthorizedAccessException($"Initialize method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
        }
        
        Logger.Info("Initializing module: {ModuleName} v{ModuleVersion} by {ModuleAuthor}, from asm: {asm}", Name, Version, Author, callingMethod?.DeclaringType?.Assembly.FullName ?? "UNKNOWN");
        if (IsFirstTimeInitialization())
        {
            IsFirstTime = true;
        }
        
        OnInitialize();
    }
    internal void Shutdown(EngineSecurityToken token) {
        StackTrace stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
        if (token == null)
        {
            throw new UnauthorizedAccessException($"Shutdown method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
        }
        
        Logger.Info("Shutting down module: {ModuleName} v{ModuleVersion} by {ModuleAuthor}, from asm: {asm}", Name, Version, Author, callingMethod?.DeclaringType?.Assembly.FullName ?? "UNKNOWN");
        
        OnShutdown();
    }
    
}

[AttributeUsage(AttributeTargets.Assembly)]
public class ModuleManifestAttribute : Attribute
{
    public string Urn { get; }
    public string Name { get; }

    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "Unknown";
    public string Description { get; set; } = "";
    public string TargetEngineVersion { get; set; } = "1.0.0";
    
    public string[] Dependencies { get; set; } = [];
    public string[] Incompatibilities { get; set; } = [];

    public ModuleManifestAttribute(string urn, string name)
    {
        Urn = urn;
        Name = name;
    }
}

/// <summary>
/// This interface is separated from the main IEngineModule so we can give it from the module manager to other systems,<br/>
/// Without exposing the full IEngineModule interface and his methods.<br/>
/// <br/>
/// NOTE: For this to work, this interface MUST NOT be changed ever!!
/// </summary>
public interface IEngineModuleInfo : IEngineModuleVersion
{
    public string Name { get; }
    public string Version { get; }
    public string Author { get; }
    public string Description { get; }
    
    /// <summary>
    /// The URNs of the modules this module depends on.<br/>
    /// This allows the engine to ensure that all required modules are loaded before this module is initialized.<br/>
    /// The user will be notified if any dependencies are missing.
    /// </summary>
    public URN[] Dependencies { get; }
    
    /// <summary>
    /// The URNs of the modules this module is incompatible with.<br/>
    /// This allows the engine to prevent loading this module if any of the incompatible modules are loaded.<br/>
    /// The user will be notified if there is a conflict, and can choose to disable the conflicting modules.
    /// </summary>
    public URN[] Incompatibilities { get; }
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
    /// It is recommended to use the format like: "[Author]://Module/[ModuleName]".<br/>
    /// Like: "Ward727://Module/RPGCreatorCore"
    /// </summary>
    public URN ModuleUrn { get; }
}