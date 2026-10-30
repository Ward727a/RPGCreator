using System.Diagnostics;
using RPGCreator.SDK.Common.Logging;


namespace RPGCreator.SDK.Modules;

/// <summary>
/// A security token used to ensure that only the engine can call certain methods on modules.<br/>
/// The engine provides this token when calling the Shutdown method on modules.<br/>
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
public struct Module : IEngineModuleInfo
{
    public ModuleManifest Manifest { get; }
    internal Type? ModuleType { get; }
    public string ModulePath { get; }
    public string ModuleHash { get; }

    public enum CertificationState : byte
    {
        /// <summary>
        /// The module is not certified.
        /// </summary>
        NotCertified = 1 << 0,
        /// <summary>
        /// The module contains a certification that has been modified.
        /// </summary>
        ModifiedCertification = 1 << 1,
        /// <summary>
        /// The module contains a valid certification, but the DLL has been modified.
        /// </summary>
        DllModified = 1 << 2,
        /// <summary>
        /// The module is certified.
        /// </summary>
        Certified = 1 << 3,
    }
    
    public readonly CertificationState Certified = CertificationState.NotCertified;

    public Module(ModuleManifest manifest, Type type, string modulePath, string moduleHash, CertificationState certified = CertificationState.NotCertified)
    {
        Manifest = manifest;
        ModuleType = type;
        ModulePath = modulePath;
        ModuleHash = moduleHash;
        Certified = certified;
    }
}

public abstract class BaseModule : IEngineModuleInfo {

    public ModuleManifest Manifest { get; }
    private bool _isFirstTime;
    public bool IsFirstTime => _isFirstTime;

    /// <summary>
    /// Check if this is the first time the module is initialized by looking for a specific file in the module folder.<br/>
    /// This can be used to run some initialization code only the first time the module is initialized, such as creating default assets or folders.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public bool IsFirstTimeInitialization()
    {
        var moduleFolder = Path.GetDirectoryName(GetType().Assembly.Location);
        
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
        
        Logger.Info("Initializing module: {ModuleName} v{ModuleVersion} by @{ModuleAuthor}, from asm: {asm}", Manifest.ModuleName, Manifest.ModuleVersion, Manifest.Authors, callingMethod?.DeclaringType?.Assembly.FullName ?? "UNKNOWN");
        if (IsFirstTimeInitialization())
        {
            _isFirstTime = true;
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
        
        Logger.Info("Shutting down module: {ModuleName} v{ModuleVersion} by @{ModuleAuthor}, from asm: {asm}", Manifest.ModuleName, Manifest.ModuleVersion, Manifest.Authors, callingMethod?.DeclaringType?.Assembly.FullName ?? "UNKNOWN");
        
        OnShutdown();
    }
}

/// <summary>
/// This interface is separated from the main IEngineModule so we can give it from the module manager to other systems,<br/>
/// Without exposing the full IEngineModule interface and his methods.<br/>
/// <br/>
/// NOTE: For this to work, this interface MUST NOT be changed ever!!
/// </summary>
public interface IEngineModuleInfo
{
    ModuleManifest Manifest { get; }
}
