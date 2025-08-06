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
using Avalonia.Input;
using Avalonia.Interactivity;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using RPGCreator.Core.Type.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Type.Assets.Tilesets;

namespace RPGCreator.UI.Content.Editor.TilesetSelectorComponents
{
    public class TilesetSelector : UserControl
    {
        public Grid Body;

        public ComboBox SelectBox { get; private set; }
        public Button SelectBoxButton { get; private set; }
        public Canvas RootTilesetCanvas { get; private set; }
        //public Button ResetRootTilesetPos { get; private set; }
        public Canvas InnerTilesetCanvas { get; private set; }
        public Border TileBorder { get; private set; }

        public Point NewInnerPosition { get; private set; } = new Point(0, 0);
        public Point LastMousePosition { get; private set; } = new Point(0, 0);
        public bool IsMovingRoot { get; private set; } = false;
        public bool HasMovedRoot { get; private set; } = false;

        private enum TilesetType
        {
            Tileset,
            Autotiling
        }
        private TilesetType CurrentTilesetType = TilesetType.Tileset;
        public ITileset SelectedTileset
        {
            get
            {
                if (SelectBox.SelectedItem is TilesetItem item)
                {
                    return item.Tileset;
                }
                return null;
            }
        }
        public Autotile SelectedAutotiles
        {
            get
            {
                if (SelectBox.SelectedItem is Autotile item)
                {
                    return item;
                }
                return null;
            }
        }

        private ITileset CurrentTemp;

        public TilesetSelector()
        {
            CreateComponents();
            Content = Body;
        }

        private void CreateComponents()
        {
            Body = new Grid
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
                RowDefinitions = new RowDefinitions("Auto, *, Auto"),
            };

            var selectBoxGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
                ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            };
            Body.Children.Add(selectBoxGrid);

            SelectBox = new ComboBox
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = App.style.Margin,
                PlaceholderText = "Choose Tileset..."
            };
            selectBoxGrid.Children.Add(SelectBox);
            SelectBox.SelectionChanged += OnSelectedTilesetChanged;

            var testBoxContent = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin
            };

            testBoxContent.Children.Add(new TextBlock
            {
                Text = "Tileset:",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            });
            testBoxContent.Children.Add(new TextBlock
            {
                Text = "TestTileset",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            });

            SelectBox.Items.Add(testBoxContent);

            SelectBoxButton = new Button
            {
                Content = "O"
            };
            selectBoxGrid.Children.Add(SelectBoxButton);
            Grid.SetColumn(SelectBoxButton, 1);

            RootTilesetCanvas = new Canvas
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Height = 256,
                Width = 256,
                ClipToBounds = true,
                // Set the background to transparent to detect mouse events properly
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(0, 0, 0, 0)),
            };

            RootTilesetCanvas.PointerPressed += OnMovingRoot;
            RootTilesetCanvas.PointerReleased += OnMovingRootEnd;
            RootTilesetCanvas.PointerMoved += OnMovingRoot;

            Body.Children.Add(RootTilesetCanvas);
            Grid.SetRow(RootTilesetCanvas, 1);

            InnerTilesetCanvas = new Canvas
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 255, 255, 255)),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };

            InnerTilesetCanvas.PointerPressed += OnSelectTile;

            Canvas.SetLeft(InnerTilesetCanvas, 0);
            Canvas.SetTop(InnerTilesetCanvas, 0);

            RootTilesetCanvas.Children.Add(InnerTilesetCanvas);

            EngineCore.Instance.Managers.Assets.Event.AddedAsset += OnAssetAdded;
            EngineCore.Instance.Managers.Assets.Event.UpdatedAsset += (sender, e) =>
            {
                if (e.Type == BaseAsset.TYPE.TILESETS)
                {
                    // If the updated asset is a tileset, we refresh the component
                    RefreshComponent();
                }
            };
            EngineCore.Instance.Managers.Assets.Event.RemovedAsset += (sender, e) =>
            {
                if (e.Type == BaseAsset.TYPE.TILESETS)
                {
                    // If the removed asset is a tileset, we refresh the component
                    RefreshComponent();
                }
            };

            RefreshComponent();
        }

        public void RefreshComponent()
        {
            var current_selected = SelectBox.SelectedIndex;
            SelectBox.Items.Clear();

#if DEBUG
            // VERY IMPORTANT: This code is only for testing purposes, it should not be used in production.
            // We check if the assets pack "TestPack" exists, if not we create it.
            if (!EngineCore.Instance.Managers.Assets.HasAssetsPack("TestPack"))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Creating TestPack assets pack for testing purposes.");
                Console.ResetColor();
                EngineCore.Instance.Managers.Assets.CreateAssetsPack("TestPack", Core.Type.Assets.BaseAssetsPack.BaseAssetsPack.PACK_TYPE.PROJECT);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Adding TestTileset to TestPack assets pack for testing purposes.");
                Console.ResetColor();

                // EngineCore.Instance.Managers.Assets.AddAsset("TestPack", new Tileset("TestTileset", 32, 32, "/home/ward/Images/RPGCreatorAssets/basic_tileset_and_assets_standard/water_and_island_tiles_v2.png"));
            }
#endif


            if (EngineCore.Instance.Data.EditedProject == null)
                return;

            var project = EngineCore.Instance.Data.EditedProject;

            project.GetAssetsType<Tileset>(Core.Type.Assets.BaseAsset.TYPE.TILESETS).ForEach(tileset =>
            {
                var item = new TilesetItem(tileset);
                if (item.Error)
                    return;
                SelectBox.Items.Add(item);
            });

            // Check if we can still select the previous selected tileset, else select the first one
            if (current_selected >= 0 && current_selected < SelectBox.Items.Count)
            {
                SelectBox.SelectedIndex = current_selected;
            }
            else if (SelectBox.Items.Count > 0)
            {
                SelectBox.SelectedIndex = 0; // Select the first item if the previous index is out of range
            }
        }

        #region EventsHandlers

        protected void OnSelectedTilesetChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Handle the event when the selected tileset changes
            // This could involve updating the UI or performing some action based on the selected tileset
            if (SelectBox.SelectedItem is TilesetItem item)
            {
                Console.WriteLine($"Selected Tileset: {item.Tileset.Name}");
                // You can add more logic here to handle the selected tileset
                InnerTilesetCanvas.Children.Clear();
                var tilesetImage = new Image
                {
                    Source = item.Tileset.GetBitmap(),
                    Width = item.Tileset.GetBitmap().Size.Width,
                    Height = item.Tileset.GetBitmap().Size.Height,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
                };
                InnerTilesetCanvas.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(20, 255, 255, 255));
                InnerTilesetCanvas.Children.Add(tilesetImage);

                if (TileBorder != null)
                {
                    CurrentTemp = item.Tileset;
                    InnerTilesetCanvas.Children.Remove(TileBorder);
                    TileBorder = null; // Clear the border when a new tileset is selected
                    EngineCore.Instance.Data.SelectedTile = null; // Clear the selected tile
                }
            }
        }

        private void Tileset_ImageChanged(object? sender, EventArgs e)
        {
            var old = SelectBox.SelectedIndex;
            SelectBox.SelectedIndex = 0;
            SelectBox.SelectedIndex = old;
        }

        protected void OnSelectTile(object? sender, PointerPressedEventArgs e)
        {

            if (!e.GetCurrentPoint(SelectBoxButton).Properties.IsLeftButtonPressed)
                return;

            Console.WriteLine("Select button clicked.");

            if (SelectedTileset == null)
            {
                Console.WriteLine("No tileset selected.");
                return;
            }

            // Here we convert the location of the click to a tile position (tileCol, tileRow)
            var position = e.GetPosition(InnerTilesetCanvas);
            int tileWidth = SelectedTileset.TileWidth;
            int tileHeight = SelectedTileset.TileHeight;

            int tileCol = (int)(position.X / tileWidth);
            int tileRow = (int)(position.Y / tileHeight);


            var tile = SelectedTileset.GetTileAt(tileCol, tileRow);

            if(tile == null)
            {
                Console.WriteLine($"No tile found at column: {tileCol}, row: {tileRow}");
                return;
            }

            // Now we can use the tileCol and tileRow to draw a square around the clicked tile
            Console.WriteLine($"Tile clicked at column: {tileCol}, row: {tileRow}");

            var tileRectangle = new Avalonia.Rect(tileCol * tileWidth, tileRow * tileHeight, tileWidth, tileHeight);

            if (TileBorder != null) // If the tileborder already exists, we just move it
            {
                Canvas.SetLeft(TileBorder, tileRectangle.X);
                Canvas.SetTop(TileBorder, tileRectangle.Y);
            }
            else
            {

                TileBorder = new Border
                {
                    Width = tileWidth,
                    Height = tileHeight,
#if DEBUG
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 0, 255, 0)),
#endif
                    BorderThickness = new Avalonia.Thickness(2),
                    BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(127, 0, 0, 0)),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
                };

                Canvas.SetLeft(TileBorder, tileRectangle.X);
                Canvas.SetTop(TileBorder, tileRectangle.Y);
                InnerTilesetCanvas.Children.Add(TileBorder);
            }

            EngineCore.Instance.Data.SelectedTile = tile;
        }
        protected void OnMovingRoot(object? sender, PointerEventArgs e)
        {
            // This event handler is triggered when the root tileset canvas is moved (Right-click and drag)

            if (e.GetCurrentPoint(RootTilesetCanvas).Properties.IsRightButtonPressed)
            {

                var CurrentPointOfInner = NewInnerPosition;

                if (!IsMovingRoot)
                {
                    // If the left button is pressed, we start moving the root tileset canvas
                    IsMovingRoot = true;
                    LastMousePosition = e.GetPosition(RootTilesetCanvas);
                    Console.WriteLine("Root tileset canvas movement started.");
                    return;
                }

                // If the left button is pressed, we can handle the movement logic here
                var position = e.GetPosition(RootTilesetCanvas);

                // Check if the mouse has moved
                if (LastMousePosition == position)
                {
                    // If the mouse has not moved, we do nothing
                    return;
                }

                Point newPosition = new Point(position.X - LastMousePosition.X, position.Y - LastMousePosition.Y) + CurrentPointOfInner;

                // Lock the new position to be within the bounds of the InnerTilesetCanvas
                newPosition = newPosition.WithX(
                    Math.Min(
                        0, 
                        Math.Max(
                            newPosition.X, 
                            (SelectedTileset.GetBitmap().Size.Width - RootTilesetCanvas.Width) * -1)
                        )
                    );
                newPosition = newPosition.WithY(
                    Math.Min(
                        0, 
                        Math.Max(
                            newPosition.Y, 
                            (SelectedTileset.GetBitmap().Size.Height - RootTilesetCanvas.Height) * -1)
                        )
                    );

                Console.WriteLine($"Root tileset canvas moved to position: {newPosition}");

                Canvas.SetLeft(InnerTilesetCanvas, newPosition.X);
                Canvas.SetTop(InnerTilesetCanvas, newPosition.Y);

                HasMovedRoot = true;
            }
        }
        protected void OnMovingRootEnd(object? sender, PointerReleasedEventArgs e)
        {
            // This event handler is triggered when the root tileset canvas movement ends (Right-click released)
            if(!e.GetCurrentPoint(RootTilesetCanvas).Properties.IsRightButtonPressed && IsMovingRoot)
            {
                NewInnerPosition = new Point(Canvas.GetLeft(InnerTilesetCanvas), Canvas.GetTop(InnerTilesetCanvas));
                // If the riht button is released, we stop moving the root tileset canvas
                IsMovingRoot = false;
                Console.WriteLine("Root tileset canvas movement ended.");
                return;
            }
        }
        protected void OnResetRootTilesetPos(object? sender, RoutedEventArgs e)
        {
            // This event handler is triggered when the reset button is clicked
            // Reset the position of the root tileset canvas to its original position
            Canvas.SetLeft(InnerTilesetCanvas, 0);
            Canvas.SetTop(InnerTilesetCanvas, 0);
            HasMovedRoot = false;
            IsMovingRoot = false;
            Console.WriteLine("Root tileset canvas position reset.");
        }

        private void OnAssetAdded(object? sender, AssetsManagerAddedAssetArgs e)
        {
            if(e.Asset.Type == BaseAsset.TYPE.TILESETS)
                RefreshComponent();
        }

        #endregion
    }
}
