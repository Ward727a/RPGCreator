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
using MonoGame.Extended.Tiled;
using RPGCreator.Core.Services;
using RPGCreator.Core.Types;
using RPGCreator.UI.Views.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.ViewModels._Editor.Windows._ProjectSettings.Dialogs
{
    public partial class TilesetEditDialogViewModel : ViewModelBase
    {

        public Tileset tileset { get; set; } = new();

        private string old_name;

        [ObservableProperty]
        private string tName;

        [ObservableProperty]
        private string tPath;

        [ObservableProperty]
        private int tDimension;

        public TilesetEditDialogViewModel(Tileset _tileset)
        {
            tileset = _tileset;
            old_name = tileset.Name;
            tName = _tileset.Name;
            tPath = _tileset.Path;
            tDimension = _tileset.Dimension;
        }

        partial void OnTNameChanged(string? oldValue, string newValue)
        {
            if (oldValue != null && newValue != null)
            {

                tileset.Name = TName;
                
            }
        }

        partial void OnTPathChanged(string value)
        {
            tileset.Path = value;
        }

        partial void OnTDimensionChanged(int value)
        {
            tileset.Dimension = value;
        }

        [RelayCommand]
        private void Confirm()
        {


            // First we check the name.
            if(tileset.Name != old_name)
            {
                if(!App.Services.GetRequiredService<EditorService>().CurrentProject!.AssetsConf.TryRenameTileset(old_name, tileset.Name))
                {
                    return;
                }
            }

            App.Services.GetRequiredService<EditorService>().CurrentProject!.AssetsConf.SetTileset(tileset.Name, tileset);
            App.Services.GetRequiredService<EditorService>().CurrentProject.AssetsConf.Save();
        }
    }
}
