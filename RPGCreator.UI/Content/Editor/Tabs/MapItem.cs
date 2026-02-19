// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Logging;

namespace RPGCreator.UI.Content.Editor.Tabs;
public class MapItem : StackPanel
{
    
    #region Events
    
    public event Action? OnMapRemoved;
    
    #endregion
    
    #region Properties
    
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<MapItem>();
    private string MapName => _mapDef.Name;
    private readonly MapDefinition _mapDef;
    
    #endregion

    #region Components

    private TextBlock? _header;
    private StackPanel? _levelsList;
    private Border? _leftLine;

    #endregion
    
    #region Constructors
    
    public MapItem(string mapName) : this(new MapDefinition(mapName))
    {
    }
    
    public MapItem(MapDefinition mapDef)
    {
        _mapDef = mapDef;
        
        Styling();
        CreateComponents();
        
        Guard.IsNotNull(_header);
        Guard.IsNotNull(_levelsList);
        Guard.IsNotNull(_leftLine);
        
        RegisterEvents();

        
        foreach(var levelChildItem in mapDef.MapDefs)
        {
            if(levelChildItem is not MapDefinition levelDef)
                continue;

            AddLevelToUi(levelDef);
        }

    }
    
    #endregion

    #region Methods
    private void Styling()
    {
        Orientation = Avalonia.Layout.Orientation.Vertical;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        Margin = App.style.Margin;
    }

    private void CreateComponents()
    {
        _header = new TextBlock
        {
            Text = MapName,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Padding = App.style.Margin,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0))
        };
        Children.Add(_header);
        
        _leftLine = new Border
        {
            BorderThickness = new Avalonia.Thickness(4, 0, 0, 0),
            BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(255, 255, 255, 255)),
        };
        Children.Add(_leftLine);

        _levelsList = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new(App.style.Margin.Left * 2, App.style.Margin.Top, App.style.Margin.Right, App.style.Margin.Bottom),
        };
        _leftLine.Child = _levelsList; // Set the LevelsList as the child of the left line
    }

    private void RegisterEvents()
    {
        PointerPressed += OnPressed;

        _header!.PointerEntered += OnHeaderHover;

        _header!.PointerExited += OnHeaderUnhover;
    }
    
    private void AddLevelToUi(MapDefinition levelDef)
    {
        var levelItem = new LevelItem(levelDef);
        levelItem.OnLevelRemoved += () => 
        {
            _levelsList!.Children.Remove(levelItem);
            _mapDef.RemoveMap(levelDef);
        };
        _levelsList!.Children.Add(levelItem);
    }
    
    #endregion

    #region EventHandlers
    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled)
            return; // If the event is already handled, do nothing
        
        var elementUnderPointer = this.InputHitTest(e.GetPosition(this));
            
        if (!Equals(elementUnderPointer, _header))
            return; // If the right click is not on the MapLevelPanel itself, do nothing
        
        var isLeftButton = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        var isRightButton = e.GetCurrentPoint(this).Properties.IsRightButtonPressed;
        
        e.Handled = true; // Mark the event as handled to prevent further processing
        if (e.ClickCount == 2 && isLeftButton)
        {
            OnOpenMap();
        } else if (isRightButton)
        {
            OnRightPressed();
        }
        else if (isLeftButton)
        {
            OnLeftPressed();
        }
        else
        {
            e.Handled = false; // If neither left nor right button, do not handle
        }
    }
    
    private void OnLeftPressed()
    {
        _leftLine!.IsVisible = !_leftLine.IsVisible; // Toggle visibility of the levels list
    }
    
    private void OnRightPressed()
    {
        EditorUiServices.MenuService.OpenContextMenu(this, [
            new MenuAction("Open Map in Editor", OnOpenMap),
            new MenuAction("Add Level", OnAddLevel),
            new MenuAction("Rename Map", OnRenameMap),
            new MenuAction("Remove Map", OnRemoveMap)
        ]);
    }
    
    private void OnHeaderHover(object? sender, PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Hand);
    }
    private void OnHeaderUnhover(object? sender, PointerEventArgs e)
    {
        Cursor = Cursor.Default;
    }

    private void OnOpenMap()
    {
        if (!RuntimeServices.MapService.LoadMap(_mapDef.Unique))
        {
            Logger.Error("Failed to load map: {MapName}", args: MapName);
            return;
        }

        Logger.Info("Opening map editor for map: {MapName}", args: MapName);
    }
    private void OnAddLevel()
    {
        
        EditorUiServices.DialogService.PromptTextAsync("Add Level", "Enter the name of the new level:", "Level Name")
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var levelName = task.Result;
                    if (string.IsNullOrWhiteSpace(levelName))
                    {
                        levelName = "New Level";
                    }

                    var levelItem = new LevelItem(levelName);
                    _levelsList!.Children.Add(levelItem);

                    _mapDef.AddMap(levelItem.Level); // Add the level to the map's levels
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        
        // Kept for now as reference for popup creation
        // var popup = new Window
        // {
        //     Title = "Add Level",
        //     Width = 300,
        //     Height = 100,
        //     WindowStartupLocation = WindowStartupLocation.CenterScreen,
        // };
        //
        // var popupPanel = new StackPanel
        // {
        //     Orientation = Avalonia.Layout.Orientation.Vertical,
        //     HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        //     VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        //     Margin = App.style.Margin
        // };
        //
        // popup.Content = popupPanel;
        //
        // var levelNameInput = new TextBox
        // {
        //     Watermark = "Level Name",
        //     HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        //     VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        //     Margin = App.style.Margin
        // };
        // popupPanel.Children.Add(levelNameInput);
        //
        // var addButton = new Button
        // {
        //     Content = "Add",
        //     HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
        //     VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        //     Margin = App.style.Margin
        // };
        //
        // popupPanel.Children.Add(addButton);
        //
        // addButton.Click += (s, e) =>
        // {
        //     // Here you would typically add the level to your data structure
        //     // For now, we just close the popup and add the level item to the list
        //     var levelItem = new LevelItem(levelNameInput.Text ?? "New Level");
        //     _levelsList!.Children.Add(levelItem);
        //
        //     _mapDef.AddMap(levelItem.Level); // Add the level to the map's levels
        //
        //     popup.Close();
        // };
        //
        // levelNameInput.KeyDown += (s, e) =>
        // {
        //     if (e.Key == Key.Enter)
        //     {
        //         // Here you would typically add the level to your data structure
        //         // For now, we just close the popup and add the level item to the list
        //         var levelItem = new LevelItem(levelNameInput.Text ?? "New Level");
        //         _levelsList!.Children.Add(levelItem);
        //
        //         _mapDef.AddMap(levelItem.Level); // Add the level to the map's levels
        //
        //         popup.Close();
        //     }
        // };
        //
        // popup.Opened += (s, e) =>
        // {
        //     // Focus the TextBox when the popup is opened
        //     levelNameInput.Focus();
        //     levelNameInput.SelectAll(); // Select all text in the TextBox
        // };
        //
        // popup.ShowDialog(EditorWindow.Instance);
    }
    private async void OnRemoveMap()
    {
        try
        {
            var confirmed = await EditorUiServices.DialogService.ConfirmAsync("Remove Map",
                $"Are you sure you want to remove this map ({MapName})?");
        
            if(confirmed)
            {
                Guard.IsNotNull(GlobalStates.ProjectState.CurrentProject, "CurrentProject");

                EngineServices.AssetsManager.GetPack(_mapDef.PackId).RemoveAsset(_mapDef.Unique);
                
                OnMapRemoved?.Invoke();
            };
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error while trying to remove map: {MapName}", args: MapName);
            await EditorUiServices.DialogService.ShowErrorAsync("Error", $"An error occurred while trying to remove the map:\n{e.Message}");
        }
    }
    private void OnRenameMap()
    {
        // Logic to rename the map
        Logger.Warning("Rename map not implemented yet for map: {MapName}", args: MapName);
    }


    #endregion
}
