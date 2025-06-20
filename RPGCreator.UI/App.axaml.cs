#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
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
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Core;
using RPGCreator.Core.Events.EventArgs;
using RPGCreator.Core.Helpers;
using RPGCreator.Core.Services;
using RPGCreator.Modals;
using RPGCreator.UI.Content.Launcher;
using RPGCreator.UI.Styles;
using System;
using System.Collections.Generic;

namespace RPGCreator.UI;

public partial class App : Application
{

    // TODO: Move this style static variable to a more appropriate place, like a StylesManager or similar.
    public static readonly BaseStyle style = new DefaultStyle();


    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        EngineCore.StartCore();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if(!EngineCore.IsCoreReady)
        {
            EngineCore.Instance.Events.CoreReady += Events_CoreReady;
            return;
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new LauncherWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView ??= new LauncherWindowControl();;
        }

        EngineCore.Instance.Events.OnUIReady(new());

        var EngineTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };

        EngineTimer.Tick += (sender, e) =>
        {
            EngineCore.Instance.Update();
        };

        EngineTimer.Start();

        base.OnFrameworkInitializationCompleted();
        this.AttachDevTools(new()
        {
            StartupScreenIndex = 1,
        });
    }

    private void Events_CoreReady(object? sender, CoreReadyArgs e)
    {
        OnFrameworkInitializationCompleted();
    }

}
