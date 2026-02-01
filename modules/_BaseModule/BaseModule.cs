using _BaseModule.Features;
using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Types;

[assembly: ModuleManifest(
    urn: "rpgc://module/base_module",
    name: "RPG Creator Base Module",
    Author = "RPG Creator Team",
    TargetEngineVersion = "1.0.0",
    Description = "Base module of RPG Creator providing essential features and functionalities.",
    Dependencies = [],
    Incompatibilities = []
)]

namespace _BaseModule;

/// <summary>
/// Base module of RPG Creator providing essential features and functionalities.<br/>
/// This module serves as the foundation for game projects, offering core components required for RPG development.<br/>
/// This is a removable part of the engine, but it is highly recommended to keep it as it provides essential features,
/// that other modules may depend on.
/// </summary>
public class BaseModule : RPGCreator.SDK.Modules.BaseModule
{
    private static readonly URN FolderUrn = new URN("rpgc", "module_path", "base_module/folder");
    
    private static readonly ScopedLogger Logger = RPGCreator.SDK.Logging.Logger.ForContext<BaseModule>();

    protected override void OnInitialize()
    {
        var folderPath = Path.GetDirectoryName(typeof(BaseModule).Assembly.Location);
        
        if(string.IsNullOrEmpty(folderPath))
            Logger.Error("Failed to register BaseModule path: folderPath is null or empty.");
        else
            EngineServices.ModulePathResolver.RegisterPath(FolderUrn, folderPath);
        
        EngineServices.FeaturesManager.RegisterEntityFeature<MovementFeature>();
        EngineServices.FeaturesManager.RegisterEntityFeature<PlayerControlledFeature>();
        
        Logger.Info("BaseModule initialized.");
    }

    protected override void OnShutdown()
    {
        Logger.Info("BaseModule shutting down.");
        EngineServices.ModulePathResolver.UnregisterPath(FolderUrn);
    }
}