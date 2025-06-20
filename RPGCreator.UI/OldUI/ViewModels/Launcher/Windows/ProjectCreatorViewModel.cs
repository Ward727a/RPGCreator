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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Core.Internal;
using RPGCreator.Core.Services;
using RPGCreator.Core.Services.Configurations;
using RPGCreator.UI.Common;
using RPGCreator.UI.OLD.Views.Launcher;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.OLD.ViewModels.Launcher.Windows
{
    internal partial class ProjectCreatorViewModel : ViewModelBase
    {
        ProjectCreatorView parent;
        [ObservableProperty]
        private string projectName = string.Empty;
        [ObservableProperty]
        private string projectDescription = string.Empty;
        [ObservableProperty]
        private string path; // This is set inside the View via the selected folder button.

        [ObservableProperty]
        private string error = "";

        [ObservableProperty]
        private string author = "";

        public int CopyrightIndexChosen = 0;

        public ObservableCollection<string> Authors { get; } = new() { };

        public List<string> Copyrights { get; } = ["No copyright", "Apache 2.0", "BSD 2.0 Clause", "BSD 3.0 Clause", "MIT", "Unlicense"];

        ProjectsConf? ProjectsConfiguration;

        [RelayCommand]
        private void CreateProject()
        {
            Error = "";
            if (ProjectsConfiguration == null)
            {
                Error = "Error: Configuration is null!";
                return;
            }

            if (string.IsNullOrEmpty(ProjectName))
            {
                Error = $"Error: {RPGCreator.UI.OLD.Lang.Resources.Launcher_ProjectCreator_ErrorNoName}";
                return;
            }
            Project project = new(ProjectName);

            if(ProjectsConfiguration.HasProject(ProjectName))
            {
                Error = $"Error: {Lang.Resources.Launcher_ProjectCreator_ErrorNameTaken}";
                return;
            }

            if (string.IsNullOrEmpty(Path))
            {
                Error = $"Error: {Lang.Resources.Launcher_ProjectCreator_ErrorNoPath}";
                return;
            }

            if(!Directory.Exists(Path))
            {
                Error = $"Error: {Lang.Resources.Launcher_ProjectCreator_ErrorPathInvalid}";
                return;
            }

            if (Directory.GetFiles(Path).Length != 0 || Directory.GetDirectories(Path).Length != 0)
            {
                Error = $"Error: {Lang.Resources.Launcher_ProjectCreator_ErrorPathNotEmpty}";
                return;
            }

            project.Path = Path;

            project.EditorVersion = AppConf.EditorVersion;
            project.Version = new Version(0, 0, 1);

            if(!string.IsNullOrEmpty(ProjectDescription))
            {
                project.Description = ProjectDescription;
            }

            if (Authors.Count > 0)
            {
                foreach (string auth in Authors)
                {
                    project.Authors.Add(auth);
                }
            }

            if(CopyrightIndexChosen != 0)
            {
                project.Copyright = Copyrights[CopyrightIndexChosen];
            }

            ProjectsConfiguration.NewProject(ref project);
            ProjectsConfiguration.Save();
            ((Window)Window.GetTopLevel(parent)).Close("Ok");
        }

        [RelayCommand]
        private void AddAuthor()
        {
            if(string.IsNullOrEmpty(Author))
                return;

            if (Authors.Contains(Author))
                return;

            Authors.Add(Author);
            Author = "";
        }

        [RelayCommand]
        private void RemoveAuthor(string author_to_remove)
        {
            if(Authors.Contains(author_to_remove))
            {
                Authors.Remove(author_to_remove);
            }
        }
        public ProjectCreatorViewModel()
        {
            //ConfigurationService config = App.Services.GetRequiredService<ConfigurationService>();

            //config.TryGetConfig<ProjectsConf>("projects", out ProjectsConfiguration);
        }
        public ProjectCreatorViewModel(ProjectCreatorView _parent) : this()
        {
            parent = _parent;
            //ConfigurationService config = App.Services.GetRequiredService<ConfigurationService>();

            //config.TryGetConfig<ProjectsConf>("projects", out ProjectsConfiguration);
        }
    }
}
