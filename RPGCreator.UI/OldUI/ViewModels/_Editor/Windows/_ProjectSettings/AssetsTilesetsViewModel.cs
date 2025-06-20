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
using RPGCreator.Core.Internal;
using RPGCreator.Core.Services;
using RPGCreator.UI.OLD.ViewModels._Editor.Windows._ProjectSettings.Dialogs;
using RPGCreator.UI.OLD.ViewModels._Editor.Windows._ProjectSettings;
using RPGCreator.UI.OLD.Views.Editor._ProjectSettings.Dialog;
using RPGCreator.UI.OLD.Views.Editor.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ursa.Controls;
using RPGCreator.Core.Types;
using Microsoft.Extensions.DependencyInjection;

namespace RPGCreator.UI.OLD.ViewModels._Editor.Windows._ProjectSettings
{
    partial class AssetsTilesetsViewModel : ViewModelSettings
    {

        private Project? project;

        public string test { get; set; } = "aaa";

        public ObservableCollection<Tileset> TilesetsItems { get; set; } = [];

        public AssetsTilesetsViewModel()
        {
            //project = App.Services.GetRequiredService<EditorService>().CurrentProject;
            //TilesetsItems = new([.. project.AssetsConf!.GetTilesets() ?? [new() { Name = "a", Path = "nan" }, new() { Name = "B", Path = "nan" }]]);
            //project.AssetsConf!.Tilesets.CollectionChanged += Tilesets_CollectionChanged;
        }

        private void Tilesets_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TilesetsItems = new([.. project.AssetsConf!.GetTilesets() ?? [new() { Name = "a", Path = "nan" }, new() { Name = "B", Path = "nan" }]]);
            OnPropertyChanged(nameof(TilesetsItems));
        }

        public override void Save()
        {

            
        }

        [RelayCommand]
        private async Task Import()
        {
            TilesetImportDialogViewModel dialogVM = new TilesetImportDialogViewModel();

            TilesetImportDialog dialog = new();
            dialog.DataContext = dialogVM;

            dialog.ShowDialog(ProjectSettings.GetInstance());
        }

        [RelayCommand]
        private async Task Edit(Tileset Tileset)
        {

            TilesetEditDialogViewModel dialog_vm = new(Tileset);

            TilesetEditDialog dialog = new TilesetEditDialog();
            dialog.DataContext = dialog_vm;

            dialog.ShowDialog(ProjectSettings.GetInstance());
        }

        [RelayCommand]
        private async void Delete(Tileset tileset)
        {

        }
    }
}
