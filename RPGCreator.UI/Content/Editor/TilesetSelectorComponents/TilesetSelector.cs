#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
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
using System;
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Extensions;

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
        
        public ListBox IntGridListBox { get; private set; }

        public Point NewInnerPosition { get; private set; } = new Point(0, 0);
        public Point LastMousePosition { get; private set; } = new Point(0, 0);
        public bool IsMovingRoot { get; private set; } = false;
        public bool HasMovedRoot { get; private set; } = false;

        public BaseTilesetDef SelectedTilesetDef
        {
            get
            {
                if (SelectBox.SelectedItem is TilesetItem item)
                {
                    return item.TilesetDef;
                }
                return null;
            }
        }

        private BaseTilesetDef CurrentTemp;

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
                // Set the background to transparent so we detect mouse events properly
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(0, 0, 0, 0)),
            };

            RootTilesetCanvas.PointerPressed += OnMovingRoot;
            RootTilesetCanvas.PointerReleased += OnMovingRootEnd;
            RootTilesetCanvas.PointerMoved += OnMovingRoot;

            Body.Children.Add(RootTilesetCanvas);
            Grid.SetRow(RootTilesetCanvas, 1);
            
            IntGridListBox = new ListBox
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                IsVisible = false
            };
            Body.Children.Add(IntGridListBox);
            Grid.SetRow(IntGridListBox, 1);

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
            
            EngineServices.AssetsManager.OnAssetRegistered += OnAssetChanges;
            EngineServices.AssetsManager.OnAssetUnregistered += OnAssetChanges;

            RefreshComponent();
        }

        public void RefreshComponent()
        {
            var current_selected = SelectBox.SelectedIndex;
            SelectBox.Items.Clear();

            #if DEBUG
            // VERY IMPORTANT: This code is only for testing purposes, it should not be used in production.
            // We check if the assets pack "TestPack" exists, if not we create it.
            // if (!EngineCore.Instance.Managers.Assets.HasAssetsPack("TestPack"))
            // {
            //     Log.Warning("Creating TestPack assets pack for testing purposes.");
            //     EngineCore.Instance.Managers.Assets.CreateAssetsPack("TestPack", Core.Type.Assets.BaseAssetsPack.BaseAssetsPack.PACK_TYPE.PROJECT); 
            // }
            #endif

            // Combine TilesetDef and AutoTilesetDef into the same array to display them together
            var tilesetDefs = EngineServices.AssetsManager.GetAssets<BaseTilesetDef>();

            foreach (var tilesetDef in tilesetDefs)
            {
                    
                var item = new TilesetItem(tilesetDef);
                if (item.Error)
                    return;
                SelectBox.Items.Add(item);
                
            }

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
                Console.WriteLine($"Selected Tileset: {item.TilesetDef.Name}");
                var def = item.TilesetDef;

                if (def is IntGridTilesetDef intgrid)
                {
                    IntGridListBox.IsVisible = true;
                    RootTilesetCanvas.IsVisible = false;

                    foreach (var intRef in intgrid.IntRefs)
                    {
                        var listItem = new TextBlock
                        {
                            Text = $"Value: {intRef.Value} - Name: {intRef.Name}"
                        };
                        IntGridListBox.Items.Add(listItem);
                    }

                    if (intgrid.IntRefs.Count <= 0)
                    {
                        var noItem = new TextBlock
                        {
                            Text = $"No IntGrid references found in this tileset."
                        };
                        IntGridListBox.Items.Add(noItem);
                    }
                    
                    return;
                }
                IntGridListBox.IsVisible = false;
                RootTilesetCanvas.IsVisible = true;
                
                // You can add more logic here to handle the selected tileset
                InnerTilesetCanvas.Children.Clear();
                var tilesetImage = new Image
                {
                    Source = EngineServices.ResourcesService.Load<Bitmap>(item.TilesetDef.ImagePath),
                    Width = item.TilesetDef.ImageWidth,
                    Height = item.TilesetDef.ImageHeight,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top
                };
                InnerTilesetCanvas.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(20, 255, 255, 255));
                InnerTilesetCanvas.Children.Add(tilesetImage);

                if (TileBorder != null)
                {
                    CurrentTemp = item.TilesetDef;
                    InnerTilesetCanvas.Children.Remove(TileBorder);
                    TileBorder = null; // Clear the border when a new tileset is selected
                    GlobalStates.EditorState.CurrentTile = null; // Clear the selected tile
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

            if (SelectedTilesetDef == null)
            {
                Console.WriteLine("No tileset selected.");
                return;
            }

            // Here we convert the location of the click to a tile position (tileCol, tileRow)
            var position = e.GetPosition(InnerTilesetCanvas);
            int tileWidth = SelectedTilesetDef.TileWidth;
            int tileHeight = SelectedTilesetDef.TileHeight;

            int tileCol = (int)(position.X / tileWidth);
            int tileRow = (int)(position.Y / tileHeight);

            var tilesetInstance = EngineServices.GameFactory.CreateInstance<ITilesetInstance>(SelectedTilesetDef);
            var tile = tilesetInstance.GetTileAt(tileCol, tileRow);

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

            GlobalStates.EditorState.CurrentTile = tile;
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
                            (SelectedTilesetDef.ImageWidth - RootTilesetCanvas.Width) * -1)
                        )
                    );
                newPosition = newPosition.WithY(
                    Math.Min(
                        0, 
                        Math.Max(
                            newPosition.Y, 
                            (SelectedTilesetDef.ImageHeight - RootTilesetCanvas.Height) * -1)
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

        private void OnAssetChanges(IBaseAssetDef newBaseAssetDef)
        {
            if(newBaseAssetDef is TilesetDef)
                RefreshComponent();
        }

        #endregion
    }
}
