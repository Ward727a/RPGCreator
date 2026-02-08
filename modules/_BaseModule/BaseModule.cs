using System.Reflection;
using _BaseModule.Features.Entity;
using _BaseModule.Features.Game;
using _BaseModule.MacroFeatures;
using _BaseModule.Registry;
using _BaseModule.UI.StatsFeature;
using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.UiService;
using RPGCreator.UI.Extensions;

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

    public static bool FirstTime = false;

    protected override void OnInitialize()
    {
        FirstTime = IsFirstTime;
        var asm = Assembly.GetExecutingAssembly();
        
        var folderPath = Path.GetDirectoryName(asm.Location);
        
        // We register the module folder path, so other part of the module can use it to reference assets inside the base module for example.
        if(string.IsNullOrEmpty(folderPath))
            Logger.Error("Failed to register BaseModule path: folderPath is null or empty.");
        else
            EngineServices.ModulePathResolver.RegisterPath(FolderUrn, folderPath);
        
        // We start by scanning the assembly for assets types, so we, and the engine more generally, can be aware of all the custom assets provided by this module, such as the stats definitions.
        // This is VERY important if you have any custom type of asset that need to be serialized, or deserialized (saved or loaded) in any way.
        // Without scanning the assembly, the engine won't be aware of these assets and won't be able to handle them properly.
        EngineServices.AssetTypeRegistry.ScanAssembly(asm);
        
        // We register the registry for the stats modifiers, which is used to store all the modifiers that can be applied to stats, such as buffs and debuffs.
        // This registry is then used by the StatsFeature to apply the modifiers to the stats of the entities.
        EngineServices.AssetsManager.RegisterRegistry(new StatModifierRegistry());
        
        // We register all entity features and game features provided by the base module.
        EngineServices.FeaturesManager.RegisterEntityFeature<MovementFeature>();
        EngineServices.FeaturesManager.RegisterEntityFeature<AnimationFeature>();
        EngineServices.FeaturesManager.RegisterEntityFeature<StatsFeature>();
        EngineServices.FeaturesManager.RegisterEntityFeature<LivingBeingMacroFeature>();
        EngineServices.FeaturesManager.RegisterGameFeature<StandardControlFeature>();
        
        // Then we can set up the custom assets menu for stats management.
        // We are doing that here, simply to allow us to 'order' the menu option in a specific way.
        // If we were to add another button like 'Items' we could want it to be before 'Stats' for example, so we would add it here before the 'Stats' button.
        UiServices.OnceServiceReady((IUiExtensionManager extensionManager) =>
        {
            extensionManager.AssetsManager().Menu((o, context) =>
            {
                context.RegisterAssetsMenuOption("Stats", () =>
                {
                    return new StatsManagement(context);
                });
            });
        });
        
        Logger.Info("BaseModule initialized.");
    }

    protected override void OnShutdown()
    {
        Logger.Info("BaseModule shutting down.");
        EngineServices.ModulePathResolver.UnregisterPath(FolderUrn);
    }
}