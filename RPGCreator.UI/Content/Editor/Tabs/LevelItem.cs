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
using Avalonia.Controls;
using Avalonia.Input;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;

namespace RPGCreator.UI.Content.Editor.Tabs;

public class LevelItem : StackPanel
{
    #region Events
    
    public event Action? OnLevelRemoved;
    
    #endregion
    
    #region Properties

    private string LevelName => Level.Name;
    public readonly MapDefinition Level;
    #endregion
    
    #region Components

    private TextBlock? _levelNameTextBlock;
    
    #endregion
    
    #region Constructors

    public LevelItem(string levelName) : this(new MapDefinition(levelName))
    {
    }

    public LevelItem(MapDefinition level)
    {
        Level = level;
        Styling();
        CreateComponents();
        RegisterEvents();
    }
    
    #endregion

    #region Methods
    private void Styling()
    {
        Orientation = Avalonia.Layout.Orientation.Horizontal;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        Margin = App.style.Margin;
        Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(60, 0, 0, 0));
    }
    
    private void CreateComponents()
    {
        _levelNameTextBlock = new TextBlock
        {
            Text = Level.Name,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
        };
        Children.Add(
            _levelNameTextBlock
        );
    }

    private void RegisterEvents()
    {
        PointerPressed += OnItemPressed;
        PointerEntered += OnItemHover;
        PointerExited += OnItemUnhover;
    }
    #endregion

    #region EventHandlers
    private void OnItemPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed) return;
            
        if (e.Handled)
            return;
            
        var elementUnderPointer = this.InputHitTest(e.GetPosition(this));
            
        if (!Equals(elementUnderPointer, this) && !Equals(elementUnderPointer, _levelNameTextBlock))
            return; 
            
        e.Handled = true; 
                
        EditorUiServices.MenuService.OpenContextMenu(this, [
            new("Remove Level", OnRemoveLevel)
        ]);
    }
    
    private void OnItemHover(object? sender, PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Hand);
    }
    
    private void OnItemUnhover(object? sender, PointerEventArgs e)
    {
        Cursor = Cursor.Default;
    }
    
    private async void OnRemoveLevel()
    {
        try
        {
            var confirmed = await EditorUiServices.DialogService.ConfirmAsync("Remove Level",
                $"Are you sure you want to remove this level ({LevelName})?");

            if (!confirmed) return;
            
            OnLevelRemoved?.Invoke();

        }
        catch (Exception e)
        {
            await EditorUiServices.DialogService.ShowErrorAsync("Error", $"An error occurred while trying to remove the level:\n{e.Message}");
        }
    }
    
    #endregion
}