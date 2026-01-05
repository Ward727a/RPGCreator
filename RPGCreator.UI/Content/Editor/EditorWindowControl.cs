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
using Avalonia.VisualTree;
using RPGCreator.UI.Content.AssetsManage;
using RPGCreator.UI.Content.Editor.Tabs;
using RPGCreator.UI.Content.Editor.TilesetSelectorComponents;
using RPGCreator.UI.Content.Editor.Toolbar;
using RPGCreator.UI.Content.Preferences;
using System;
using System.Diagnostics;
using System.Numerics;
using RPGCreator.Core.Types.Windows;
using RPGCreator.RTP;
using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Content.Editor.LeftPanel;

namespace RPGCreator.UI.Content.Editor
{
    public class EditorWindowControl : UserControl
    {
        private Window _Host => (Window)this.GetVisualRoot()!;

        private EditorGame? game = (EditorGame)EngineServices.GameProvider.GameInstance;
        private AvaloniaInside.MonoGame.MonoGameControl MonoGameScreen;

        private TilesetSelector tilesetSelector;
        private Vector2 _LastTilePlacePos;
        private Vector2 _LastTilePreviewPos;

        private bool _placingTile = false; // Flag to indicate if a tile is being placed
        private Grid _mainGrid;
        private Menu _menuBar;

        public EditorWindowControl()
        {

            if (game == null)
            {
                throw new InvalidOperationException("EditorGame is not initialized. Make sure to initialize the game before using this control.");
            }

            CreateComponents();
            RegisterEvents();
            Content = _mainGrid;
        }

        private void CreateComponents()
        {
            _mainGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                RowDefinitions = new RowDefinitions("Auto, *, 1, Auto")
            };

            #region MenuBar
            _menuBar = new Menu
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            };
            _mainGrid.Children.Add(_menuBar);
            Grid.SetRow(_menuBar, 0);

            var fileMenuItem = new MenuItem
            {
                Header = "File"
            };
            _menuBar.Items.Add(fileMenuItem);

            var newFileMenuItem = new MenuItem
            {
                Header = "New"
            };
            fileMenuItem.Items.Add(newFileMenuItem);
            var openFileMenuItem = new MenuItem
            {
                Header = "Open..."
            };
            var openProjectFolderMenuItem = new MenuItem
            {
                Header = "Open Project Folder"
            };
            openProjectFolderMenuItem.Click += (_, _) =>
            {
                // Open the project folder in the file explorer
                if(EngineState.ProjectState.CurrentProject == null)
                {
                    Logger.Error("No project is currently loaded.");
                    return;
                }
                var projectPath = EngineState.ProjectState.CurrentProject.Path;
                if (!string.IsNullOrEmpty(projectPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = projectPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    Logger.Error("Project path is not set.");
                }
            };
            openFileMenuItem.Items.Add(openProjectFolderMenuItem);
            fileMenuItem.Items.Add(openFileMenuItem);
            var saveFileMenuItem = new MenuItem
            {
                Header = "Save"
            };
            fileMenuItem.Items.Add(saveFileMenuItem);
            saveFileMenuItem.Click += (_, _) =>
            {
                var loadingModal = new LoadDialog("Saving...", "Saving the current project, please wait...");
                loadingModal.ShowDialog(_Host).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // Handle any errors that occurred while saving
                        Console.WriteLine("Error saving project: " + t.Exception?.Message);
                    }
                    else
                    {
                        // Successfully saved the project
                        Console.WriteLine("Project saved successfully.");
                    }
                });
                if(EngineState.ProjectState.CurrentProject == null)
                {
                    Logger.Error("No project is currently loaded.");
                    return;
                }
                EngineState.ProjectState.CurrentProject.Save();
            };
            var closeFileMenuItem = new MenuItem
            {
                Header = "Close"
            };
            fileMenuItem.Items.Add(closeFileMenuItem);
            var exportFileMenuItem = new MenuItem
            {
                Header = "Export"
            };
            fileMenuItem.Items.Add(exportFileMenuItem);

            var editMenuItem = new MenuItem
            {
                Header = "Edit"
            };
            _menuBar.Items.Add(editMenuItem);
            var openTestDialogMenuItem = new MenuItem
            {
                Header = "Open Test Dialog"
            };
            openTestDialogMenuItem.Click += (_, _) =>
            {
                // Open the testing dialog
                var testDialog = new TestingDialog();
                testDialog.ShowDialog(_Host).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // Handle any errors that occurred while showing the dialog
                        Console.WriteLine("Error showing test dialog: " + t.Exception?.Message);
                    }
                });
            };
            editMenuItem.Items.Add(openTestDialogMenuItem);
            var undoEditMenuItem = new MenuItem
            {
                Header = "Undo"
            };
            editMenuItem.Items.Add(undoEditMenuItem);
            var redoEditMenuItem = new MenuItem
            {
                Header = "Redo"
            };
            editMenuItem.Items.Add(redoEditMenuItem);
            var projectSettingsMenuItem = new MenuItem
            {
                Header = "Project Settings"
            };
            editMenuItem.Items.Add(projectSettingsMenuItem);
            var preferencesMenuItem = new MenuItem
            {
                Header = "Preferences"
            };
            preferencesMenuItem.Click += PreferencesMenuItem_Click;
            editMenuItem.Items.Add(preferencesMenuItem);

            var assetsMenuItem = new MenuItem
            {
                Header = "Assets"
            };
            _menuBar.Items.Add(assetsMenuItem);
            var addAssetMenuItem = new MenuItem
            {
                Header = "Add..."
            };
            assetsMenuItem.Items.Add(addAssetMenuItem);
            var manageAssetsMenuItem = new MenuItem
            {
                Header = "Manage Assets"
            };
            manageAssetsMenuItem.Click += ManageAssetsMenuItem_Click;
            assetsMenuItem.Items.Add(manageAssetsMenuItem);
            var importAssetsMenuItem = new MenuItem
            {
                Header = "Import Assets"
            };
            assetsMenuItem.Items.Add(importAssetsMenuItem);
            var exportAssetsMenuItem = new MenuItem
            {
                Header = "Export Assets"
            };
            assetsMenuItem.Items.Add(exportAssetsMenuItem);
            var helpMenuItem = new MenuItem
            {
                Header = "Help"
            };
            _menuBar.Items.Add(helpMenuItem);
            var aboutMenuItem = new MenuItem
            {
                Header = "About"
            };
            helpMenuItem.Items.Add(aboutMenuItem);
            var documentationMenuItem = new MenuItem
            {
                Header = "Documentation"
            };
            helpMenuItem.Items.Add(documentationMenuItem);
            var reportIssueMenuItem = new MenuItem
            {
                Header = "Report Issue"
            };
            helpMenuItem.Items.Add(reportIssueMenuItem);
            #endregion

            var ContentGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto"),
                RowDefinitions = new RowDefinitions("*, Auto"),
            };
            _mainGrid.Children.Add(ContentGrid);
            Grid.SetRow(ContentGrid, 1);

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

            tabControl.Items.Add(MapLevelTab.CreateTab(_Host));
            tabControl.Items.Add(MapEditor.CreateTab(_Host));

            var separatorLeftPanel0 = new Separator
            {
                Margin = App.style.Margin,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray),
            };
            LeftPanel.Children.Add(separatorLeftPanel0);
            Grid.SetRow(separatorLeftPanel0, 1);

            tilesetSelector = new TilesetSelector();
            LeftPanel.Children.Add(tilesetSelector);
            Grid.SetRow(tilesetSelector, 2);

            #endregion

            var RightPanel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Width = 300,
            };
            ContentGrid.Children.Add(RightPanel);
            Grid.SetColumn(RightPanel, 2);

            var BottomPanel = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Height = 200,
                RowDefinitions = new RowDefinitions("*"),
            };
            ContentGrid.Children.Add(BottomPanel);
            Grid.SetColumn(BottomPanel, 1);
            Grid.SetColumnSpan(BottomPanel, 2);
            Grid.SetRow(BottomPanel, 1);

            var subBottomBorder = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0)),
                Margin = new Thickness(4),
                Padding = new Thickness(4),
                BorderThickness = new Thickness(1),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray),
                CornerRadius = new CornerRadius(2),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };
            BottomPanel.Children.Add(subBottomBorder);

            var SubBottomPanel = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Orientation = Avalonia.Layout.Orientation.Vertical,
            };
            subBottomBorder.Child = SubBottomPanel;

            var CenterGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                RowDefinitions = new("auto, *")
            };
            ContentGrid.Children.Add(CenterGrid);
            Grid.SetColumn(CenterGrid, 1);

            var toolbar = new ToolbarControl();
            CenterGrid.Children.Add(toolbar);

            // This is used to contain the MonoGame screen inside it's bounds.
            // If we don't do this, the MonoGame screen will not be able to resize properly and some dirty tricks would be needed.
            // AKA: Adding a margin to the MonoGame screen, then removing it "down" property to each position when needed, etc...
            // This is a cleaner way to do it.
            var monogameGrid = new Grid
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
            };
            CenterGrid.Children.Add(monogameGrid);
            Grid.SetRow(monogameGrid, 1);

            MonoGameScreen = new AvaloniaInside.MonoGame.MonoGameControl
            {
                Game = game,
            };
            monogameGrid.Children.Add(MonoGameScreen);
        }

        private void RegisterEvents()
        {
            MonoGameScreen.PointerEntered += (s, e) =>
            {
                game.CanUseMouse = true;
            };

            MonoGameScreen.PointerExited += (s, e) =>
            {
                game.CanUseMouse = false;
            };

            MonoGameScreen.PointerPressed += MonoGameScreen_PointerPressed;
            MonoGameScreen.PointerReleased += MonoGameScreen_PointerReleased;
            MonoGameScreen.PointerMoved += MonoGameScreen_PointerMoved;
            MonoGameScreen.PointerExited += MonoGameScreen_PointerExited;
            MonoGameScreen.KeyDown += MonoGameScreenOnKeyDown;

            game.OnDraw += () =>
            {
                var visualPosition = (MonoGameScreen.TransformToVisual(_Host)?.Transform(new Avalonia.Point(0, 0))).GetValueOrDefault();
    
                int newWidth = (int)MonoGameScreen.Bounds.Width;
                int newHeight = (int)MonoGameScreen.Bounds.Height;
                var newPos = new Microsoft.Xna.Framework.Point(
                    (int)(_Host.Position.X + 8 + 300), 
                    (int)(visualPosition.Y + _Host.Position.Y + 1 + _menuBar.Bounds.Height)
                );

                if (game.GraphicsDevice.PresentationParameters.BackBufferWidth != newWidth || 
                    game.GraphicsDevice.PresentationParameters.BackBufferHeight != newHeight)
                {
                    game.GraphicsDevice.PresentationParameters.BackBufferWidth = newWidth;
                    game.GraphicsDevice.PresentationParameters.BackBufferHeight = newHeight;
                    game._graphics.ApplyChanges();
                }

                if (game.Window.Position != newPos)
                {
                    game.Window.Position = newPos;
                }
                
            };

        }

        private void MonoGameScreenOnKeyDown(object? sender, KeyEventArgs e)
        {
            // EngineCore.Instance.Events.OnRTPKeyPressed(e);
        }

        private void MonoGameScreen_PointerExited(object? sender, PointerEventArgs e)
        {
            EngineServices.BrushManager.ClearPreview(); // Clear the preview when the mouse exits the MonoGame screen
        }

        private void ManageAssetsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            // Open the assets management window
            var assetsManageWindow = new AssetsManageWindow();
            assetsManageWindow.ShowDialog(_Host).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Handle any errors that occurred while showing the assets management window
                    Console.WriteLine("Error showing assets management window: " + t.Exception?.Message);
                }
            });
        }

        private void PreferencesMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            // Open the preferences window
            var preferencesWindow = new PreferencesWindow();

            preferencesWindow.ShowDialog(_Host).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Handle any errors that occurred while showing the preferences window
                    Console.WriteLine("Error showing preferences window: " + t.Exception?.Message);
                }
            });
        }

        private void MonoGameScreen_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(MonoGameScreen).Properties.IsLeftButtonPressed)
            {
                var mgPosition = e.GetPosition(MonoGameScreen);
                var position = new Vector2((float)mgPosition.X, (float)mgPosition.Y);
                // Adjust the position to account for the MonoGameScreen's margin (12px)
                EngineServices.BrushManager.ClickAt(new Vector2(position.X, position.Y));
                EngineState.BrushState.LastDrawAt = EngineServices.BrushManager.NormalizedPositionToTile(position);
                EngineState.BrushState.IsDrawing = true; // Set the flag to indicate that a tile is being placed
            }
        }

        private void MonoGameScreen_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (e.GetCurrentPoint(MonoGameScreen).Properties.IsLeftButtonPressed && _placingTile)
            {
                EngineState.BrushState.IsDrawing = false; // Reset the flag when the tile placement is done
                EngineState.BrushState.LastDrawAt = new(-1, -1); // Reset the last tile position
            }
        }

        private void MonoGameScreen_PointerMoved(object? sender, PointerEventArgs e)
        {
            var mgPosition = e.GetPosition(MonoGameScreen);
            var position = new Vector2((float)mgPosition.X, (float)mgPosition.Y);
            if (_placingTile && e.GetCurrentPoint(MonoGameScreen).Properties.IsLeftButtonPressed)
            {

                // Check if the mouse position has at least moved one tile from the last position
                var normalizedCurrentPosition = EngineServices.BrushManager.NormalizedPositionToTile(position);

                if(normalizedCurrentPosition != (_LastTilePlacePos))
                {
                    // If the position has changed, update the last position
                    EngineState.BrushState.LastDrawAt = normalizedCurrentPosition;
                }
                else
                {
                    // If the position hasn't changed, do not place a tile again
                    return;
                }

                // Adjust the position to account for the MonoGameScreen's margin (12px)
                EngineServices.BrushManager.ClickAt(position);
            }
            
            {
                // Check if the mouse position has at least moved one tile from the last position
                var normalizedCurrentPosition = EngineServices.BrushManager.NormalizedPositionToTile(position);

                if (normalizedCurrentPosition != (EngineState.BrushState.LastDrawAt))
                {
                    // If the position has changed, update the last position
                    EngineState.BrushState.LastDrawAt = normalizedCurrentPosition;
                }
                else
                {
                    // If the position hasn't changed, do not place a tile again
                    return;
                }

                EngineServices.BrushManager.PreviewAt(position);
            }

        }
    }
}
