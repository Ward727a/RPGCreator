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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RPGCreator.Core;
using RPGCreator.Core.Types;
using RPGCreator.Core.Types.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Editor.Context;

namespace RPGCreator.UI.Content.Editor.Tabs
{
    public class MapLevelTab : UserControl, ITab
    {

        private MapEditorContext _context;
        private ScrollViewer _Scroller;
        private StackPanel _MapList;

        private class PopupMap : Window
        {
            public PopupMap()
            {
                Title = "Add Map";
                Width = 300;
                Height = 200;
                Content = new TextBox
                {
                    Watermark = "Map Name",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = App.style.Margin
                };
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private class PopupLevel : Window
        {
            public PopupLevel()
            {
                Title = "Add Level";
                Width = 300;
                Height = 200;
                Content = new TextBox
                {
                    Watermark = "Level Name",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = App.style.Margin
                };
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private class MapItem : StackPanel
        {
            private StackPanel LevelsList;
            private Border _leftLine;
            public string MapName { get; set; } = "New Map";
            public int MapId { get; set; } = 0;
            public MapDefinition MapDef;
            private MapEditorContext _context;

            public MapItem(MapDefinition mapDef, MapEditorContext ctx) : this(mapDef.Name, ctx)
            {
                MapDef = mapDef;

                foreach(MapDefinition level in mapDef.MapDefs)
                {
                    var levelItem = new LevelItem(level.Name);
                    LevelsList.Children.Add(levelItem);
                }

            }

            public MapItem(string MapName, MapEditorContext ctx) : this(ctx)
            {
                this.MapName = MapName;
                InitUI();
            }

            public MapItem(MapEditorContext ctx)
            {
                _context = ctx;
                InitUI();
            }

            private void InitUI()
            {
                Orientation = Avalonia.Layout.Orientation.Vertical;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                Margin = App.style.Margin;
                MapDef = new(MapName);
                EngineCore.Instance.Data.EditedProject.GameData.Maps.Add(MapDef);

                var header = new TextBlock
                {
                    Text = MapName,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    Padding = App.style.Margin,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0))
                };

                PointerPressed += (s, e) =>
                {

                    // If double left click, open the map editor
                    if (e.ClickCount == 2 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    {
                        if (e.Handled)
                            return; // If the event is already handled, do nothing
                        var elementUnderPointer = this.InputHitTest(e.GetPosition(this));
                        if (elementUnderPointer != header)
                            return; // If the double click is not on the MapItem itself, do nothing
                        e.Handled = true; // Mark the event as handled to prevent further processing

                        _context.Map = MapDef; // Set the edited map to the current map

                        // Open the map editor
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Opening map editor for map: {MapName}");
                        Console.ResetColor();
                    } else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                    {
                        if (e.Handled)
                            return; // If the event is already handled, do nothing

                        var elementUnderPointer = this.InputHitTest(e.GetPosition(this));
                        if (elementUnderPointer != header)
                            return; // If the right click is not on the MapLevelPanel itself, do nothing

                        e.Handled = true; // Mark the event as handled to prevent further processing

                        if (GlobalStaticUIData.CurrentContext != null)
                        {
                            GlobalStaticUIData.CloseContext();
                        }

                        GlobalStaticUIData.CurrentContext = new ContextMenu();

                        var openMapItem = new MenuItem { Header = "Open Map in Editor" };
                        openMapItem.Click += (s, e) =>
                        {

                            _context.Map = MapDef; // Set the edited map to the current map
                            // Open the map editor
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Opening map editor for map: {MapName}");
                            Console.ResetColor();
                            //EditorWindow.Instance.OpenMapEditor(Map);
                        };
                        (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(openMapItem);

                        var addLevelItem = new MenuItem { Header = "Add Level" };
                        addLevelItem.Click += (s, e) => OnAddLevel();
                        (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(addLevelItem);

                        var renameMapItem = new MenuItem { Header = "Rename Map" };
                        renameMapItem.Click += (s, e) => OnRenameMap();
                        (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(renameMapItem);

                        var removeMapItem = new MenuItem { Header = "Remove Map" };
                        removeMapItem.Click += (s, e) => OnRemoveMap();
                        (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(removeMapItem);

                        GlobalStaticUIData.OpenContext(this);
                    }
                    else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    {
                        // Handle left click if needed
                        // For example, you could focus the map or select it
                        // For now it will just "close" the list of levels if it was open
                        if (e.Handled)
                            return; // If the event is already handled, do nothing

                        var elementUnderPointer = this.InputHitTest(e.GetPosition(this));
                        if (elementUnderPointer != header)
                            return; // If the right click is not on the MapLevelPanel itself, do nothing

                        e.Handled = true; // Mark the event as handled to prevent further processing

                        if (_leftLine.IsVisible)
                        {
                            _leftLine.IsVisible = false; // Hide the levels list if it was visible
                        }
                        else
                        {
                            _leftLine.IsVisible = true; // Show the levels list if it was hidden
                        }
                    }
                };

                header.PointerEntered += (s, e) =>
                {
                    // Change cursor to hand when hovering over the MapItem
                    this.Cursor = Avalonia.Input.Cursor.Parse("Hand");
                };

                header.PointerExited += (s, e) =>
                {
                    // Reset cursor when not hovering over the MapItem
                    this.Cursor = Avalonia.Input.Cursor.Default;
                };

                _leftLine = new Border
                {
                    BorderThickness = new Avalonia.Thickness(4, 0, 0, 0),
                    BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(255, 255, 255, 255)),
                };

                LevelsList = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    Margin = new(App.style.Margin.Left * 2, App.style.Margin.Top, App.style.Margin.Right, App.style.Margin.Bottom),
                };

                Children.Add(
                    header
                );
                Children.Add(_leftLine);
                _leftLine.Child = LevelsList; // Set the LevelsList as the child of the left line
            }

            public void OnAddLevel()
            {
                var popup = new Window
                {
                    Title = "Add Level",
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                };

                var popupPanel = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = App.style.Margin
                };

                popup.Content = popupPanel;

                var levelNameInput = new TextBox
                {
                    Watermark = "Level Name",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = App.style.Margin
                };
                popupPanel.Children.Add(levelNameInput);

                var addButton = new Button
                {
                    Content = "Add",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Margin = App.style.Margin
                };

                popupPanel.Children.Add(addButton);

                addButton.Click += (s, e) =>
                {
                    // Here you would typically add the level to your data structure
                    // For now, we just close the popup and add the level item to the list
                    var levelItem = new LevelItem(levelNameInput.Text ?? "New Level");
                    LevelsList.Children.Add(levelItem);

                    MapDef.AddMap(levelItem.Level); // Add the level to the map's levels

                    popup.Close();
                };

                levelNameInput.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        // Here you would typically add the level to your data structure
                        // For now, we just close the popup and add the level item to the list
                        var levelItem = new LevelItem(levelNameInput.Text ?? "New Level");
                        LevelsList.Children.Add(levelItem);

                        MapDef.AddMap(levelItem.Level); // Add the level to the map's levels

                        popup.Close();
                    }
                };

                popup.Opened += (s, e) =>
                {
                    // Focus the TextBox when the popup is opened
                    levelNameInput.Focus();
                    levelNameInput.SelectAll(); // Select all text in the TextBox
                };

                popup.ShowDialog(EditorWindow.Instance);
            }

            public void OnRemoveMap()
            {
                var confirmDialog = new ConfirmDialog("Remove Map", $"Are you sure you want to remove this map ({MapName})?");

                confirmDialog.Confirmed += () =>
                {
                    // Logic to remove the map
                    var parent = this.Parent as StackPanel;
                    parent?.Children.Remove(this);
                    EngineCore.Instance.Data.EditedProject.GameData.Maps.Remove(MapDef); // Remove the map from the project data
                };

                confirmDialog.ShowDialog(EditorWindow.Instance);
            }

            public void OnRenameMap()
            {
                // Logic to rename the map
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Rename Map clicked. This feature is not implemented yet.");
                Console.ResetColor();
            }
        }

        private class LevelItem : StackPanel
        {
            public string LevelName { get; set; } = "New Level";
            public int LevelId { get; set; } = 0;
            public MapDefinition Level;
            public LevelItem(string levelName)
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                Margin = App.style.Margin;
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0));

                Level = new(levelName);

                var text =
                    new TextBlock
                    {
                        Text = levelName,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = App.style.Margin,
                    };
                Children.Add(
                    text
                );

                this.PointerPressed += (s, e) =>
                {
                    if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                    {
                        if (e.Handled)
                            return; // If the event is already handled, do nothing
                        var elementUnderPointer = this.InputHitTest(e.GetPosition(this));
                        if (elementUnderPointer != this && elementUnderPointer != text)
                            return; // If the right click is not on the LevelItem itself, do nothing
                        e.Handled = true; // Mark the event as handled to prevent further processing
                        if (GlobalStaticUIData.CurrentContext != null)
                        {
                            GlobalStaticUIData.CloseContext();
                        }
                        GlobalStaticUIData.CurrentContext = new ContextMenu();
                        var removeLevelItem = new MenuItem { Header = "Remove Level" };
                        removeLevelItem.Click += (s, e) => OnRemoveLevel();
                        (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(removeLevelItem);
                        GlobalStaticUIData.OpenContext(this);
                    }
                };
                PointerEntered += (s, e) =>
                {
                    // Change cursor to hand when hovering over the MapItem
                    this.Cursor = Avalonia.Input.Cursor.Parse("Hand");
                };

                PointerExited += (s, e) =>
                {
                    // Reset cursor when not hovering over the MapItem
                    this.Cursor = Avalonia.Input.Cursor.Default;
                };

            }

            public LevelItem()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal;
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                Margin = App.style.Margin;
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0));

                Level = new(LevelName);

                var text =
                    new TextBlock
                    {
                        Text = LevelName,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        Margin = App.style.Margin,
                    };
                Children.Add(
                    text
                );

                this.PointerPressed += (s, e) =>
                {
                    if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
                    {
                        if(e.Handled)
                            return; // If the event is already handled, do nothing
                        var elementUnderPointer = this.InputHitTest(e.GetPosition(this));
                        if (elementUnderPointer != this && elementUnderPointer != text)
                            return; // If the right click is not on the LevelItem itself, do nothing
                        e.Handled = true; // Mark the event as handled to prevent further processing
                        if (GlobalStaticUIData.CurrentContext != null)
                        {
                            GlobalStaticUIData.CloseContext();
                        }
                        GlobalStaticUIData.CurrentContext = new ContextMenu();
                        var removeLevelItem = new MenuItem { Header = "Remove Level" };
                        removeLevelItem.Click += (s, e) => OnRemoveLevel();
                        (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(removeLevelItem);
                        GlobalStaticUIData.OpenContext(this);
                    }
                };
                PointerEntered += (s, e) =>
                {
                    // Change cursor to hand when hovering over the MapItem
                    this.Cursor = Avalonia.Input.Cursor.Parse("Hand");
                };

                PointerExited += (s, e) =>
                {
                    // Reset cursor when not hovering over the MapItem
                    this.Cursor = Avalonia.Input.Cursor.Default;
                };

            }

            public void OnRemoveLevel()
            {

                var confirmDialog = new ConfirmDialog("Remove Level", $"Are you sure you want to remove this level ({LevelName})?");

                confirmDialog.Confirmed += () =>
                {
                    // Logic to remove the level
                    // This can be overridden in derived classes if needed
                    var parent = this.Parent as StackPanel;
                    parent?.Children.Remove(this);
                };

                confirmDialog.ShowDialog(EditorWindow.Instance);
            }
        }

        public MapLevelTab(MapEditorContext ctx) : base()
        {
            _context = ctx;
            var cont = new Grid
            {
                Margin = App.style.Margin,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                RowDefinitions = new RowDefinitions("Auto,*"),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0))
            };
            this.Content = cont;

            var searchBox = new TextBox
            {
                Watermark = "Search ([map] | [level] | [level]@[map])",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            cont.Children.Add(searchBox);

            var grid = new Grid
            {
                Margin = App.style.Margin,
                RowDefinitions = new("*")
            };
            cont.Children.Add(grid);

            _Scroller = new ScrollViewer
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin,
                ClipToBounds = true,
            };

            _MapList = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Margin = App.style.Margin
            };

            _Scroller.Content = _MapList;
            cont.Children.Add(_Scroller);
            Grid.SetRow(_Scroller, 1);

            var textMapTest = new MapItem(_context);
            _MapList.Children.Add(textMapTest);

            cont.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(cont).Properties.IsRightButtonPressed)
                {
                    var elementUnderPointer = cont.InputHitTest(e.GetPosition(cont));

                    if (elementUnderPointer != cont && elementUnderPointer != _Scroller && elementUnderPointer.GetType().Name != "ScrollContentPresenter")
                        return; // If the right click is not on the MapLevelPanel itself, do nothing

                    e.Handled = true;

                    GlobalStaticUIData.CloseContext();
                    GlobalStaticUIData.CurrentContext = new ContextMenu();
                    var addMapItem = new MenuItem { Header = "Add Map" };
                    addMapItem.Click += (s, e) => OnCreateNewMap();
                    (GlobalStaticUIData.CurrentContext as ContextMenu).Items.Add(addMapItem);
                    GlobalStaticUIData.OpenContext(cont);
                }
            };
        }

        private void OnCreateNewMap()
        {
            var win = this.GetVisualRoot() as Window;
            var panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            var popup = new Window
            {
                Title = "Add Map",
                Width = 300,
                Height = 100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = panel,
            };

            var mapNameInput = new TextBox
            {
                Watermark = "Map Name",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            panel.Children.Add(mapNameInput);

            var addButton = new Button
            {
                Content = "Add",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            panel.Children.Add(addButton);

            addButton.Click += (s, e) =>
            {

                var map = new MapItem(mapNameInput.Text ?? "New Map", _context);
                _MapList.Children.Add(map);

                popup.Close();
            };
            mapNameInput.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {

                    var map = new MapItem(mapNameInput.Text ?? "New Map", _context);
                    _MapList.Children.Add(map);

                    popup.Close();
                }
            };

            popup.Opened += (s, e) =>
            {
                // Focus the TextBox when the popup is opened
                mapNameInput.Focus();
                mapNameInput.SelectAll(); // Select all text in the TextBox
            };

            popup.ShowDialog(EditorWindow.Instance);
        }
        public static TabItem CreateTab(Window host, MapEditorContext? ctx = null)
        {
            // Need to do this to avoid create "multiple" instances...
            // In fact, even if we don't create multiple instances, it still crashes the application due to creating "multiple" instances of the same control.
            // Weird issues, but well, this works for now.
            var tab = new TabItem
            {
                Header = "Map/Level"
            };

            tab.Content = new MapLevelTab(ctx);

            return tab;
        }
    }
}
