using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Modules.UIModule;
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
        UIExtensionManager.RegisterExtension(UIRegion.AssetsManager, (control, o) =>
        {
            // Customize the control in the Assets Manager region
            // For example, here we change the background color and edit the title
            //if (control is Window window)
            //{
            //    window.Title = "Test Module";
            //    window.Background = Avalonia.Media.Brushes.LightGray;
            //}
        });

        EngineServices.ModulePathResolver.RegisterPath(new URN("Ward727", "module", "TestModule/Folder"),
            Path.GetDirectoryName(typeof(TestModule).Assembly.Location));
    }

    protected override void OnShutdown()
    {
    }
}