#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LiveMarkdown.Avalonia;
using RPGCreator.UI.Content.Launcher;
using RPGCreator.UI.Styles;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.UI.Ressources;
using RPGCreator.UI.Services;
using RPGCreator.UI.UiService;
using IResourceService = RPGCreator.SDK.Resources.IResourceService;

namespace RPGCreator.UI;

public class App : Application
{
    // TODO: Move this style static variable to a more appropriate place, like a StylesManager or similar.
    public static readonly BaseStyle style = new DefaultStyle();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        EditorUiServices.DialogService = new DialogService();
        EditorUiServices.MenuService = new MenuService();
        EditorUiServices.NotificationService = new NotificationService();
        EditorUiServices.ExtensionManager = new UiExtensionManager();
        EditorUiServices.DocService = new DocService();
        EditorUiServices.MonogameViewport = new MonogameViewportService();
        EngineServices.OnceServiceReady((IResourceService ResourcesService) =>
        {
            ResourcesService.RegisterLoader<Avalonia.Media.Imaging.Bitmap>(new AvaloniaBitmapLoader());
        });
    }

    
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);
        MarkdownNode.Register<MathInlineNode>();
        MarkdownNode.Register<MathBlockNode>();
        IconProvider.Current
            .Register<MaterialDesignIconProvider>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new LauncherWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView ??= new LauncherWindowControl();;
        }

        base.OnFrameworkInitializationCompleted();
        this.AttachDevTools(new()
        {
            StartupScreenIndex = 1,
        });
    }

}
