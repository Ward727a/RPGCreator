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
using RPGCreator.UI.ViewModels;
using RPGCreator.UI.Views;
using System;
using System.Collections.Generic;

namespace RPGCreator.UI;

public partial class App : Application
{

    // Structure

    /// <summary>
    /// Allow to add service to the ServicesProvider.
    /// </summary>
    /// 
    private readonly Dictionary<Type, Type[]> SingletonByInterfaceTable = new()
    {
    };

    private readonly Type[] SingletonsTable =
    [
        typeof(ErrorDialog),
        typeof(LauncherWindow),
        typeof(LauncherView),
        typeof(EditorService)

    ];

    private readonly Type[] TransientsTable = 
    [

    ];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static IServiceProvider Services { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

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

        LoadServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<LauncherWindow>();
            if (desktop.MainWindow != null)
            {
                //FolderManager folderManager = Services.GetRequiredService<FolderManager>();
                //ConfigurationService s = Services.GetRequiredService<ConfigurationService>();
                desktop.MainWindow.DataContext = new LauncherViewModel(desktop.MainWindow);
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Services.GetService<LauncherView>();
            if(singleViewPlatform.MainView != null)
            {
                //FolderManager folderManager = Services.GetRequiredService<FolderManager>();
                singleViewPlatform.MainView.DataContext = new LauncherViewModel((Window)singleViewPlatform.MainView);
            };
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
    }

    private void Events_CoreReady(object? sender, CoreReadyArgs e)
    {
        OnFrameworkInitializationCompleted();
    }

    private void LoadServices()
    {
        ServiceCollection services = new();

        foreach (KeyValuePair<Type, Type[]> pair in SingletonByInterfaceTable)
        {
            foreach (Type type in pair.Value)
            {
                services.AddSingleton(pair.Key, type);
            }
        }

        foreach (Type singleton in SingletonsTable)
        {
            services.AddSingleton(singleton);
        }

        foreach (Type transient in TransientsTable)
        {
            services.AddTransient(transient);
        }

        Services = services.BuildServiceProvider();
        
    }
}
