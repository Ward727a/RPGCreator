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
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RPGCreator.Core;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Type.Assets.Tilesets;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.TilesetEditor
{
    public class TilesetEditorWindowControl : UserControl
    {

        public event Action? TilesetSaved;

        private bool _FromWindow;
        public Grid Body { get; private set; }
        public TilesetDef TilesetDefinition { get; private set; }

        public Grid ImageContainer { get; private set; }
        public Image ImagePreview { get; private set; }
        public PathPicker ImagePick { get; private set; }

        public StackPanel MainPanel { get; private set; }
        public TextBox NameInput { get; private set; }
        public TextBox TileHeightInput { get; private set; }
        public TextBox TileWidthInput { get; private set; }
        public ComboBox AssetPackChoice { get; private set; }
        
        public StackPanel ExamplesTilesPanel { get; private set; }
        public Button ExamplesGenerateTilesButton { get; private set; }
        public TextSeparator ExamplesTilesLabel { get; private set; }
        public Grid ExamplesTilesGrid { get; private set; }

        public TilesetEditorWindowControl(TilesetDef tilesetDefinition, bool FromWindow = false)
        {
            TilesetDefinition = tilesetDefinition ?? throw new ArgumentNullException(nameof(tilesetDefinition), "Tileset cannot be null");
            CreateComponents();
            this.Content = Body;
            _FromWindow = FromWindow;
        }

        private void CreateComponents()
        {
            Body = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
                ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("Auto, *"),
                RowDefinitions = new Avalonia.Controls.RowDefinitions("*, Auto")
            };

            CreateImagePick();
            CreateInputFields();
            CreateExampleTiles();
            CreateFooter();

        }
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
        private void CreateImagePick()
        {

            ImageContainer = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
                RowDefinitions = new Avalonia.Controls.RowDefinitions("Auto, Auto"),
            };
            Body.Children.Add(ImageContainer);
            Grid.SetColumn(ImageContainer, 0);

            // Preview of the image picked if any
            ImagePreview = new Image
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                MaxWidth = 200,
                MaxHeight = 200,
                Width = 200,
                Height = 200,
                Margin = new Avalonia.Thickness(4)
            };
            ImageContainer.Children.Add(ImagePreview);
            Grid.SetRow(ImagePreview, 0);
            RenderOptions.SetBitmapInterpolationMode(ImagePreview, Avalonia.Media.Imaging.BitmapInterpolationMode.None);

        }

        private void CreateInputFields()
        {
            MainPanel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = new Avalonia.Thickness(10)
            };
            Body.Children.Add(MainPanel);
            Grid.SetColumn(MainPanel, 1);

            ImagePick = new PathPicker
            {
                Title = "Choose image...",
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
                UsePickerType = UsePickerTypes.OpenFile,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,

            };

            if(File.Exists(TilesetDefinition.ImagePath))
            {
                ImagePick.SelectedPathsText = TilesetDefinition.ImagePath;
                ImagePick.SelectedPaths.Append(TilesetDefinition.ImagePath);
                ImagePreview.Source = TilesetDefinition.GetBitmap();
            }

            MainPanel.Children.Add(ImagePick);

            ImagePick.PropertyChanged += (sender, e) =>
            {
                if (e.Property.Name == nameof(PathPicker.SelectedPaths))
                {
                    if (ImagePick.SelectedPaths[0] != null && ImagePick.SelectedPaths[0] is string path)
                    {
                        if (File.Exists(path))
                        {
                            try
                            {
                                ImagePreview.Source = new Bitmap(path);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading image: {ex.Message}");
                                ImagePreview.Source = null;
                            }
                        }
                    }
                    else
                    {
                        ImagePreview.Source = null;
                    }
                }
            };

            NameInput = new TextBox
            {
                Watermark = "Tileset Name",
                Text = TilesetDefinition.Name,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            MainPanel.Children.Add(NameInput);

            TileHeightInput = new TextBox
            {
                Watermark = "Tile Height",
                Text = TilesetDefinition.TileHeight > 0 ? TilesetDefinition.TileHeight.ToString() : string.Empty,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            MainPanel.Children.Add(TileHeightInput);
            TileWidthInput = new TextBox
            {
                Watermark = "Tile Width",
                Text = TilesetDefinition.TileWidth > 0 ? TilesetDefinition.TileWidth.ToString() : string.Empty,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            MainPanel.Children.Add(TileWidthInput);

            AssetPackChoice = new ComboBox
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
            };

            var index = 0;
            var currentPack = -1;
            // Populate the ComboBox with available asset packs
            foreach (var packName in EngineCore.Instance.Managers.Assets.GetAssetsPackNames())
            {
                AssetPackChoice.Items.Add(packName);
                if (TilesetDefinition.PackName != null && TilesetDefinition.PackName == packName)
                {
                    currentPack = index;
                }
                index++;
            }
            MainPanel.Children.Add(AssetPackChoice);

            // Set the selected index if a pack is found
            if (currentPack >= 0)
            {
                AssetPackChoice.SelectedIndex = currentPack;
            }
            else
            {
                AssetPackChoice.SelectedIndex = 0;
            }
        }

        private void CreateExampleTiles()
        {
            ExamplesTilesPanel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };

            ImageContainer.Children.Add(ExamplesTilesPanel);
            Grid.SetRow(ExamplesTilesPanel, 1);

            ExamplesTilesLabel = new TextSeparator
            {
                Text = "Example Tiles",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            ExamplesTilesPanel.Children.Add(ExamplesTilesLabel);

            ExamplesTilesGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
                Margin = new Avalonia.Thickness(10),
                ColumnDefinitions = new Avalonia.Controls.ColumnDefinitions("Auto, Auto, Auto, Auto"),
                RowDefinitions = new Avalonia.Controls.RowDefinitions("Auto, Auto, Auto, Auto")
            };

            ExamplesTilesPanel.Children.Add(ExamplesTilesGrid);

            ExamplesGenerateTilesButton = new Button
            {
                Content = "Generate Example Tiles",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            ExamplesGenerateTilesButton.Click += (sender, e) =>
            {
                RefreshExamplesTiles();
            };
            ExamplesTilesPanel.Children.Add(ExamplesGenerateTilesButton);

        }

        private void RefreshExamplesTiles()
        {
            // Clear the existing tiles
            ExamplesTilesGrid.Children.Clear();

            var image = ImagePreview.Source as Bitmap;

            if (image != null)
            {

                var maxRows = (int)(image.Size.Height / TilesetDefinition.TileHeight);
                var maxColumns = (int)(image.Size.Width / TilesetDefinition.TileWidth);

                for (int i = 0; i < 8; i++)
                {
                    var row = (int)((i / 4));
                    var column = i % 4;
                    // Create a new Image control for the tile
                    var tileImage = new Image
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Width = TilesetDefinition.TileWidth,
                        Height = TilesetDefinition.TileHeight,
                        Margin = new Avalonia.Thickness(2)
                    };

                    // Calculate the position of the tile in the tileset image
                    var x = (i % 4) * TilesetDefinition.TileWidth;
                    var y = (i / 4) * TilesetDefinition.TileHeight;
                    // Create a cropped bitmap for the tile
                    var croppedBitmap = new CroppedBitmap(image, new PixelRect(x, y, TilesetDefinition.TileWidth, TilesetDefinition.TileHeight));
                    tileImage.Source = croppedBitmap;
                    // Add the tile image to the grid
                    ExamplesTilesGrid.Children.Add(tileImage);
                    Grid.SetRow(tileImage, row);
                    Grid.SetColumn(tileImage, column);
                }
            }
        }

        private void CreateFooter()
        {
            // Create a footer with a save button
            var footer = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Margin = new Avalonia.Thickness(10)
            };
            Body.Children.Add(footer);
            Grid.SetRow(footer, 2);
            Grid.SetColumnSpan(footer, 2);
            var saveButton = new Button
            {
                Content = "Save Tileset",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            saveButton.Click += SaveTileset;
            footer.Children.Add(saveButton);
        }

        private void SaveTileset(object? sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(NameInput.Text))
            {
                // TODO Message box to inform the user that the name cannot be empty
                return;
            }
            if (ImagePick.SelectedPaths.Count == 0 || !File.Exists(ImagePick.SelectedPaths[0]))
            {
                // TODO Message box to inform the user that the image path is invalid
                return;
            }

            // Update the tileset properties
            TilesetDefinition.Name = NameInput.Text;
            TilesetDefinition.TileHeight = int.TryParse(TileHeightInput.Text, out var height) ? height : 0;
            TilesetDefinition.TileWidth = int.TryParse(TileWidthInput.Text, out var width) ? width : 0;
            TilesetDefinition.ImagePath = ImagePick.SelectedPaths[0];
            TilesetDefinition.PackName = AssetPackChoice.SelectedItem as string;

            if (EngineCore.Instance.Managers.Assets.TryGetAssetsPack(TilesetDefinition.PackName, out var pack))
            {
                // Check if the tileset already exists in the pack
                if (pack != null)
                {
                    if (pack.AssetsCache.Any(a => a.Value.Unique == TilesetDefinition.Unique))
                    {
                        // If it exists, update the existing tileset
                        pack.UpdateAsset(TilesetDefinition);
                        Console.WriteLine($"Tileset Updated: {TilesetDefinition.Name}, Width: {TilesetDefinition.TileWidth}, Height: {TilesetDefinition.TileHeight}, Asset Pack: {TilesetDefinition.PackName}");
                    }
                    else
                    {
                        // If it doesn't exist, add the new tileset to the pack
                        pack.AddAsset(TilesetDefinition);
                        Console.WriteLine($"Tileset Added: {TilesetDefinition.Name}, Width: {TilesetDefinition.TileWidth}, Height: {TilesetDefinition.TileHeight}, Asset Pack: {TilesetDefinition.PackName}");
                    }
                }

                Console.WriteLine($"New Tileset Created: {TilesetDefinition.Name}, Width: {TilesetDefinition.TileWidth}, Height: {TilesetDefinition.TileHeight}, Asset Pack: {TilesetDefinition.PackName}");
            } else
            {
                throw new Exception("Couldn't get the pack from the pack name... INTERNAL ERROR!");
            }
            
            EngineCore.Instance.Managers.Assets.TilesetRegistry.Register(TilesetDefinition);

            TilesetSaved?.Invoke();
        }
    }
}
