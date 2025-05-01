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
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RPGCreator.UI.ViewModels._Editor.Windows._ProjectSettings.Dialogs
{
    public partial class TilesetImportDialogViewModel : ViewModelBase
    {

        [ObservableProperty]
        private string _path = string.Empty;
        [ObservableProperty]
        private string _name = string.Empty;
        [ObservableProperty]
        private int _dimension;
        [ObservableProperty]
        private bool _hardlink;

        private string FileName;
        private int FileDimension;

        [ObservableProperty]
        private string _error = "";

        public bool HasError => !string.IsNullOrEmpty(Error);

        [ObservableProperty]
        private bool _HasPath = false;

        partial void OnPathChanged(string value)
        {

            HasPath = false;
            if(Path != null && File.Exists(Path) && (Path.EndsWith(".png") | Path.EndsWith(".jpg") | Path.EndsWith(".jpeg")))
                HasPath = true;

            if(string.IsNullOrEmpty(Name) || Name == FileName)
            {
                FileName = System.IO.Path.GetFileName(Path)!.Replace("_", " ");
                FileName = FileName.Replace(".png", "").Replace(".jpg", "").Replace(".jpeg", "");
                Name = FileName;
            }

            if(Dimension == 0 || Dimension == FileDimension)
            {
                // This allow us to get a "[number]px" in the file name, this is not 100% sure to be the dimension, but it could be.
                var regex = new Regex(@"\b(\d+)\s*px\b", RegexOptions.IgnoreCase);
                var match = regex.Match(FileName);

                if (match.Success)
                {
                    FileDimension = int.Parse(match.Groups[1].Value);
                    Dimension = FileDimension;
                }
            }

        }

        [RelayCommand]
        private void Confirm()
        {

            if (!HasPath && string.IsNullOrEmpty(Name) && Dimension != 0)
            {
                Error = "Either Path, Name is empty and/or Dimension is equal to 0 or empty.";
            }

            if (App.Services.GetRequiredService<EditorService>().HasProject && App.Services.GetRequiredService<EditorService>().CurrentProject!.HasAssets)
            {
                if(App.Services.GetRequiredService<EditorService>().CurrentProject!.AssetsConf!.TryAddTileset(Path, Name, Dimension, Hardlink))
                {
                    
                } else
                {
                    Error = "Couldn't create / import this tileset.";
                }
            }
        }

        [RelayCommand]
        private void Cancel()
        { }
    }
}
