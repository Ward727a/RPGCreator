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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Core.Services;
using RPGCreator.UI.OLD.ViewModels._Editor.Windows._ProjectSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.OLD.ViewModels._Editor.Windows._ProjectSettings
{
    partial class InformationViewModel : ViewModelSettings
    {
        [ObservableProperty]
        private string _projectName;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private int major;
        [ObservableProperty]
        private int minor;
        [ObservableProperty]
        private int build;
        [ObservableProperty]
        private int revision;

        public InformationViewModel() {
            //_projectName = App.Services.GetRequiredService<EditorService>().CurrentProject?.Name ?? "No title (ERROR)";
            //_description = App.Services.GetRequiredService<EditorService>().CurrentProject?.Description ?? string.Empty;

            //major = App.Services.GetRequiredService<EditorService>().CurrentProject?.Version.Major ?? 0;
            //minor = App.Services.GetRequiredService<EditorService>().CurrentProject?.Version.Minor ?? 0;
            //build = App.Services.GetRequiredService<EditorService>().CurrentProject?.Version.Build ?? 0;
            //revision = App.Services.GetRequiredService<EditorService>().CurrentProject?.Version.Revision ?? 0;
        }

        public override void Save()
        {
            //App.Services.GetRequiredService<ConfigurationService>().Save("projects");
        }

        partial void OnDescriptionChanged(string value)
        {
            //App.Services.GetRequiredService<EditorService>().CurrentProject!.Description = value;
        }

        partial void OnProjectNameChanged(string value)
        {
            //App.Services.GetRequiredService<EditorService>().CurrentProject!.Name = value;
        }

        partial void OnMajorChanged(int value)
        {
            SaveVersion();
        }

        partial void OnMinorChanged(int value)
        {
            SaveVersion();
        }

        partial void OnBuildChanged(int value)
        {
            SaveVersion();
        }

        partial void OnRevisionChanged(int value)
        {
            SaveVersion();
        }


        private void SaveVersion()
        {
            if (Major < 0)
            {
                Major = 0;
                return;
            }

            if (Minor < 0)
            {
                //App.Services.GetRequiredService<EditorService>().CurrentProject!.Version = new Version(Major, 0);
            }
            else if (Build < 0)
            {
                //App.Services.GetRequiredService<EditorService>().CurrentProject!.Version = new Version(Major, Minor);
            }
            else if (Revision < 0)
            {
                //App.Services.GetRequiredService<EditorService>().CurrentProject!.Version = new Version(Major, Minor, Build);
            } else
            {
                //App.Services.GetRequiredService<EditorService>().CurrentProject!.Version = new Version(Major, Minor, Build, Revision);
            }
        }
    }
}
