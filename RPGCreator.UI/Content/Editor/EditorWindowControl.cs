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
using Avalonia.VisualTree;
using RPGCreator.UI.Content.AssetsManage;
using RPGCreator.UI.Content.Editor.Tabs;
using RPGCreator.UI.Content.Editor.TilesetSelectorComponents;
using RPGCreator.UI.Content.Editor.Toolbar;
using RPGCreator.UI.Content.Preferences;
using System;
using System.Numerics;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using RPGCreator.RTP.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Common;
using RPGCreator.UI.Common.Bridge;
using RPGCreator.UI.Content.Editor.LayersListComponents;
using RPGCreator.UI.Content.Editor.LeftPanel;
using Size = System.Drawing.Size;
using Vector = Avalonia.Vector;

namespace RPGCreator.UI.Content.Editor
{
    public class EditorWindowControl : UserControl
    {
        private Window _Host => (Window)TopLevel.GetTopLevel(this)!;

        // private EditorGame? game = (EditorGame)RuntimeServices.GameRunner;
        private MonoGameControl MonoGameScreen;

        private TilesetSelector tilesetSelector;
        private Vector2 _LastTilePlacePos;
        private Vector2 _LastTilePreviewPos;

        private bool _placingTile = false; // Flag to indicate if a tile is being placed
        private Grid _mainGrid;
        private EditorMenuBar _menuBar;

        
        
        private WriteableBitmap _realTimePlayerWriteableBitmap = new(new PixelSize(1172, 827), new Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Premul);

        private Image mgImage;
        
        Grid monogameGrid;
        public EditorWindowControl()
        {

            // if (game == null)
            // {
            //     throw new InvalidOperationException("EditorGame is not initialized. Make sure to initialize the game before using this control.");
            // }

            CreateComponents();
            RegisterEvents();
            Content = _mainGrid;
            
            Logger.Debug(((Grid)mgImage.Parent).DesiredSize.ToString());
            
            EditorUiServices.MonogameViewport.OnCoreReady += () =>
            {
                using (var buf = _realTimePlayerWriteableBitmap.Lock())
                {
                    var viewport = EditorUiServices.MonogameViewport.CreateNewViewport("Editor MonoGame Viewport", buf.Address,
                        new SDK.Types.Size(1172, 827));
                    viewport?.LockToImageControl("MonoGameImage");
                    monogameGrid.SizeChanged += (_, _) =>
                    {
                        _realTimePlayerWriteableBitmap = new WriteableBitmap(new PixelSize((int)monogameGrid.Bounds.Width, (int)monogameGrid.Bounds.Height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, Avalonia.Platform.AlphaFormat.Premul);
                        viewport?.Resize(new SDK.Types.Size((int)monogameGrid.Bounds.Width, (int)monogameGrid.Bounds.Height));
                        mgImage.Source = _realTimePlayerWriteableBitmap;
                    };
                    viewport?.OnceUpdatedDo(()=>
                    {
                        Dispatcher.UIThread.Post(()=>
                        {
                            mgImage.InvalidateVisual();
                        }, priority: DispatcherPriority.Render);
                    });
                    viewport?.DoNewFrameAction += () =>
                    {
                        using (var buf = _realTimePlayerWriteableBitmap.Lock())
                        {
                            return buf.Address;
                        }
                    };
                }
            };
        }

        private void CreateComponents()
        {
            _mainGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                RowDefinitions = new RowDefinitions("Auto, *"),
                ColumnDefinitions = new ColumnDefinitions("Auto, *"),
                RowSpacing = 4,
                ColumnSpacing = 4
            };
            EditorUiServices.MonogameViewport.Initialize();
            var RenderCore = ((MonogameViewportService)EditorUiServices.MonogameViewport)._core;
            var mgBrain = new MonoGameControl()
            {
                Game = RenderCore,
                Opacity = 0, // Make the control invisible
                IsHitTestVisible = false,
                Focusable = false,
                IsTabStop = false,
                Width = 1,
                Height = 1,
            };
            _mainGrid.Children.Add(mgBrain);

            #region MenuBar
            _menuBar = new EditorMenuBar()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            };
            _mainGrid.Children.Add(_menuBar);
            Grid.SetRow(_menuBar, 0);
            #endregion
            
            var shortcutsBar = new EditorShortcutsBar()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = App.style.Margin
            };
            _mainGrid.Children.Add(shortcutsBar);
            Grid.SetRow(shortcutsBar, 0);
            Grid.SetColumn(shortcutsBar, 1);

            var ContentGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                ColumnDefinitions = new ColumnDefinitions("Auto, *"),
                RowDefinitions = new RowDefinitions("*"),
                RowSpacing = 4,
                ColumnSpacing = 4
            };
            _mainGrid.Children.Add(ContentGrid);
            Grid.SetRow(ContentGrid, 1);
            Grid.SetColumnSpan(ContentGrid, 2);

            #region LeftBar
            var LeftPanel = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Width = 300,
                RowDefinitions = new RowDefinitions("*, 1, *"),
            };
            var LeftPanel2 = new EditorLeftPanelControl();
            
            ContentGrid.Children.Add(LeftPanel2);
            Grid.SetColumn(LeftPanel2, 0);
            Grid.SetRowSpan(LeftPanel2, 2);

            var tabControl = new TabControl
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
            };
            LeftPanel.Children.Add(tabControl);

            tabControl.Items.Add(new TabItem()
            {
                Header = "Maps",
                Content = new MapLevelTab()
            });
            tabControl.Items.Add(new TabItem()
            {
                Header = "Layers",
                Content = new LayersListComponent()
            });

            var separatorLeftPanel0 = new Separator
            {
                Margin = App.style.Margin,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Gray),
            };
            LeftPanel.Children.Add(separatorLeftPanel0);
            Grid.SetRow(separatorLeftPanel0, 1);

            tilesetSelector = new TilesetSelector();
            LeftPanel.Children.Add(tilesetSelector);
            Grid.SetRow(tilesetSelector, 2);

            #endregion

            var CenterGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                RowDefinitions = new("auto, *"),
                RowSpacing = 4,
                Margin = App.style.Margin
            };
            ContentGrid.Children.Add(CenterGrid);
            Grid.SetColumn(CenterGrid, 1);

            var toolbar = new EditorToolsBar();
            CenterGrid.Children.Add(toolbar);
            
            monogameGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
            CenterGrid.Children.Add(monogameGrid);
            Grid.SetRow(monogameGrid, 1);

            
            mgImage = new Image
            {
                Source = _realTimePlayerWriteableBitmap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Focusable = true,
                Name = "MonoGameImage",
            };
            monogameGrid.Children.Add(mgImage);
            
            monogameGrid.Children.Add(new TextBlock()
            {
                Text = "PREVIEW - This is not the final design of the editor, lots of changes are still planned!",
                FontWeight = FontWeight.Bold,
                FontSize = 24
            });
        }


        private readonly AvaloniaKeyboardBridge _keyboardBridge = new();
        private readonly AvaloniaMouseBridge _mouseBridge = new();
        
        private void RegisterEvents()
        {
            _mouseBridge.RegisterEvents(mgImage);
            _keyboardBridge.RegisterEvents(mgImage);
        }
    }
}
