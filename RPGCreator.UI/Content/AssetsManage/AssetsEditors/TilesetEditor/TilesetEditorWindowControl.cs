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
using Avalonia.Media.Imaging;
using RPGCreator.Core;
using RPGCreator.Core.Type.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.TilesetEditor
{
    public class TilesetEditorWindowControl : UserControl
    {

        public Grid Body { get; private set; }
        public Tileset Tileset { get; private set; }

        public TilesetEditorWindowControl(Tileset tileset)
        {
            Tileset = tileset ?? throw new ArgumentNullException(nameof(tileset), "Tileset cannot be null");
            CreateComponents();
            this.Content = Body;
        }

        private void CreateComponents()
        {
            Body = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
                ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("Auto, *"),
            };

            CreateImagePick();
            CreateInputFields();

        }

        private void CreateImagePick()
        {

            // TODO: Voir pour implementer la création de nouveaux tilesets.
            // Si j'ai le temps, faut que j'vois aussi pour le système d'autotiling (Création du nouveaux types, etc...)
            // Pour l'autotiling, je pourrait voir pour créé un type contenant une liste de tilesets utilisés pour l'autotiling,
            // Puis à chaque placement d'une tile, il vérifie si l'autotile le contient, et si oui, il place la tile correspondante.
            // OU
            // Voir pour implémenter un nouveau système dans le TilesetSelector qui permet de choisir un autotile,
            // l'utilisateur pourrait assigner des tiles "basique" sur une image généré depuis les datas de l'autotile, puis les placer,
            // Comme ça y'aura pas besoin de check chaque autotiles à chaque placement de tile,
            // Mais qu'un seul, celui que l'utilisateur a choisi.
            // A réfléchir, mais je pense que c'est la meilleure solution.
            //
            //
            // Pour l'instant je vais bossé sur la création de nouveaux tilesets,
            // Puis je créé le type d'autotile qui contiendra une liste de tilesets,
            // Et je créé un système de base de création d'autotile,
            // Je modifierai le TilesetSelector pour qu'il puisse afficher les autotiles et en séléctionner les tiles,
            // Et enfin je verrai pour implémenter le système de placement d'autotile.

            var imageContainer = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
                RowDefinitions = new Avalonia.Controls.RowDefinitions("Auto, Auto"),
            };
            Body.Children.Add(imageContainer);
            Grid.SetColumn(imageContainer, 0);

            // Preview of the image picked if any
            var imagePreview = new Image
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                MaxWidth = 200,
                MaxHeight = 200,
                Margin = new Avalonia.Thickness(4)
            };
            imageContainer.Children.Add(imagePreview);
            Grid.SetRow(imagePreview, 0);
            RenderOptions.SetBitmapInterpolationMode(imagePreview, Avalonia.Media.Imaging.BitmapInterpolationMode.None);

            var imagePick = new PathPicker
            {

                MaxWidth = 200,
                MaxHeight = 200,
                Title = "Choose image...",
                UsePickerType = UsePickerTypes.OpenFile,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,

            };
            imageContainer.Children.Add(imagePick);
            Grid.SetRow(imagePick, 1);

            imagePick.PropertyChanged += (sender, e) =>
            {
                if (e.Property.Name == nameof(PathPicker.SelectedPaths))
                {
                    if (imagePick.SelectedPaths[0] != null && imagePick.SelectedPaths[0] is string path)
                    {
                        if (File.Exists(path))
                        {
                            // Load the image and set it to the preview
                            try
                            {
                                imagePreview.Source = new Bitmap(path);
                            }
                            catch (Exception ex)
                            {
                                // Handle the exception if the image cannot be loaded
                                Console.WriteLine($"Error loading image: {ex.Message}");
                                imagePreview.Source = null;
                            }
                        }
                    }
                    else
                    {
                        imagePreview.Source = null;
                    }
                }
            };
        }

        private void CreateInputFields()
        {
            var mainPanel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = new Avalonia.Thickness(10)
            };
            Body.Children.Add(mainPanel);
            Grid.SetColumn(mainPanel, 1);

            // Add input fields for tileset properties
            var nameInput = new TextBox
            {
                Watermark= "Tileset Name",
                Text = Tileset.Name,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            mainPanel.Children.Add(nameInput);

            var tileHeightInput = new TextBox
            {
                Watermark = "Tile Height",
                Text = Tileset.tile_height > 0 ? Tileset.tile_height.ToString() : string.Empty,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            mainPanel.Children.Add(tileHeightInput);
            var tileWidthInput = new TextBox
            {
                Watermark = "Tile Width",
                Text = Tileset.tile_width > 0 ? Tileset.tile_width.ToString() : string.Empty,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            mainPanel.Children.Add(tileWidthInput);

            var assetPackChoice = new ComboBox
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
            };

            var index = 0;
            var currentPack = -1;
            // Populate the ComboBox with available asset packs
            foreach (var packName in EngineCore.Instance.Managers.AssetsPack.GetAssetsPacksNames())
            {
                assetPackChoice.Items.Add(packName);
                if (Tileset.PackName != null && Tileset.PackName == packName)
                {
                    currentPack = index;
                }
                index++;
            }
            mainPanel.Children.Add(assetPackChoice);

            // Set the selected index if a pack is found
            if (currentPack >= 0)
            {
                assetPackChoice.SelectedIndex = currentPack;
            }
            else
            {
                assetPackChoice.SelectedIndex = 0; // Default to the first pack if none matches
            }
        }
    }
}
