using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Types;

[assembly: ModuleManifest(
    urn: "rpgc://module/test_module",
    name: "Test Module",
    Author = "Your Name",
    TargetEngineVersion = "1.0.0",
    Description = "A test module for RPG Creator.",
    Dependencies = [],
    Incompatibilities = []
)]

namespace TestModule;

public class TestModule : BaseModule
{
    protected override void OnInitialize()
    {
        // Initialization code here
        Logger.Info("Test Module initialized.");
        
        // Register a UI extension for the Assets Manager region.
        //
        // This will be called whenever the Assets Manager UI is created / opened or refreshed in the engine.
        // This is a very powerful feature that allows you to customize and extend the engine's UI as you see fit.
        //
        // Some UIRegion also have specific context objects that are passed as the second parameter 'o' in the callback.
        // More information about those context objects can be found in the documentation (Not yet available).
        //
        // You can even add your own custom regions in the engine's UI using the UIExtensionManager.RegisterRegion method.
        // Just make sure to choose unique region names to avoid conflicts with other modules.
        // 
        // Important note: Here we are using the UiServices.OnceServiceReady method to be sure that this code is executed only on the editor
        // and not while the game is running (or else it would cause crashes since the UI services and Avalonia are not available in the game).
        // UiServices.OnceServiceReady((IUiExtensionManager extensionManager) =>
        // {
        //     extensionManager.AssetsManager().Configure((o, context) =>
        //     {
        //         if (o is Window window)
        //         {
        //             window.Background = Brushes.Aqua;
        //         }
        //     }).Menu((o, context) =>
        //     {
        //         context.RegisterAssetsMenuSeparator();
        //         context.RegisterAssetsMenuOption("test_option", () =>
        //         {
        //             Logger.Info("Test option clicked!");
        //             return new UserControl();
        //         });
        //     });
        // });

        EngineServices.ModulePathResolver.RegisterPath(new URN("Ward727", "module", "TestModule/Folder"),
            Path.GetDirectoryName(typeof(TestModule).Assembly.Location));
    }

    protected override void OnShutdown()
    {
    }
}