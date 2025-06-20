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
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Core;
using RPGCreator.Core.Configs.Helpers;
using RPGCreator.Core.Managers.ProjectsManager.Events;
using RPGCreator.Core.Type.Project;

//using RPGCreator.Core.Helpers;
//using RPGCreator.Core.Internal;
//using RPGCreator.Core.Services;
//using RPGCreator.Core.Services.Configurations;
using RPGCreator.Modals;
using RPGCreator.UI.Common;
using RPGCreator.UI.OLD.Models;
using RPGCreator.UI.OLD.ViewModels._Editor;
using RPGCreator.UI.OLD.Views.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RPGCreator.UI.OLD.ViewModels;

public partial class LauncherViewModel : ViewModelBase
{

    //// Services
    //private readonly ModalService _modalService;
    //private readonly ProjectsConf? _projectsConf;

    // Model
    private readonly LauncherModel _launcherModel;

    // Commands
    public ICommand OpenProjectCreator { get; }

    // Public variable
    public string Greeting => "Welcome to Avalonia!";
    public SolidColorBrush BackgroundItemColor => new SolidColorBrush(Color.FromArgb(20, 255, 255, 255), 1);
    public ObservableCollection<BaseProject> ItemsList { get; set; }

    public LauncherViewModel()
    {
        
        ItemsList = new ObservableCollection<BaseProject>(new List<BaseProject> { new BaseProject("Test") { Description = "template description", Path = "C:/My/Path/To/Project", IsArchived = false, IsFavorite = true, Copyright = "Unlicense", EditorVersion = new Version(1, 0, 0, 0), Version = new Version(1, 0, 0), Name = "My Project"} });
    }

    public LauncherViewModel(Window parent)
    {
        //_modalService = new ModalService(parent);
        ProjectsConf _projectsConf = EngineCore.Instance.Configs.GetConfig<ProjectsConf>("ProjectsConf");

        _launcherModel = new(parent);
        OpenProjectCreator = new RelayCommand(_launcherModel.OpenSecondWindow);

        if (_projectsConf != null)
        {
            ItemsList = _projectsConf.Projects;
        }
        else
            throw new Exception("Couldn't get the project configuration object.");

        // This is just an example of how to show a error message dialog, this still being worked on.
        //_modalService.ShowCustom<ErrorDialog>(new Dictionary<string, object>(){ { "title", "Error!" }, { "content", "My custom error" } });
    }

    [RelayCommand]
    private void EditProject(string project_name)
    {
        EventHandler<ProjectsManagerLoadedProjectArgs> Handler = null!;

        Handler = (object? sender, ProjectsManagerLoadedProjectArgs args) =>
        {
            EngineCore.Instance.Managers.Projects.Events.LoadedProject -= Handler;
            
            if(args.HasError)
                return;

            if (args.LoadedProject == null)
                return;

            Editor editor = new Editor();
            editor.DataContext = new EditorWindowViewModel();
            editor.Show();
        };

        EngineCore.Instance.Managers.Projects.Events.LoadedProject += Handler;

        EngineCore.Instance.Managers.Projects.LoadProject(project_name);

        //ConfigurationService config = App.Services.GetRequiredService<ConfigurationService>();

        //config.TryGetConfig<ProjectsConf>("projects", out ProjectsConf? project_conf);

        //Project? edited_project = null;

        //if (project_conf != null)
        //{
        //    if(project_conf.HasProject(project_name))
        //    {
        //        project_conf.TryGetProject(project_name, out edited_project);
        //    }
        //}

        //if (edited_project != null)
        //{
        //    Editor editor = new Editor(edited_project);
        //    editor.DataContext = new EditorWindowViewModel(edited_project);
        //    editor.Show();
        //}
    }
}
