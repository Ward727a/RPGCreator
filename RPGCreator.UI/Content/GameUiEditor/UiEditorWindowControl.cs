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

using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaInside.MonoGame;
using CommunityToolkit.Diagnostics;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.GameUI;
using RPGCreator.UI.Content.GameUiEditor.Components;
using RPGCreator.UI.Test;
using Ursa.Controls;

namespace RPGCreator.UI.Content.GameUiEditor;

public sealed class UiEditorWindowControl : UserControl
{
    
    private const int ExplorerRow = 0;
    private const int PreviewerRow = ExplorerRow + 2;
    private const int PropertiesRow = PreviewerRow + 2;
    
    
    private IGameUiRunner _uiPreviewer;
    
    private UiTopMenu TopMenu { get; set; } = null!;
    private GridSplitter LeftSplitter { get; set; } = null!;
    private UiExplorer Explorer { get; set; } = null!;
    private GridSplitter RightSplitter { get; set; } = null!;
    private UiProperties Properties { get; set; } = null!;
    private MonoGameControlTest PreviewerControlTest { get; set; } = null!;
    
    private Grid MainBody { get; set; } = null!;
    private Grid ContentGrid { get; set; } = null!;

    public UiEditorWindowControl(IGameUiRunner previewer)
    {
        _uiPreviewer = previewer;
        Guard.IsAssignableToType<Game>(_uiPreviewer);
        
        CreateComponents();
        RegisterEvents();
    }

    private void CreateComponents()
    {
        MainBody = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
            ColumnDefinitions = new ColumnDefinitions("*")
        };
        Content = MainBody;
        
        TopMenu = new UiTopMenu();
        MainBody.Children.Add(TopMenu);
        Grid.SetRow(TopMenu, 0);

        ContentGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("350, 2, *, 2, 350"),
        };
        MainBody.Children.Add(ContentGrid);
        Grid.SetRow(ContentGrid, 1);
        
        Explorer = new UiExplorer();
        ContentGrid.Children.Add(Explorer);
        Grid.SetColumn(Explorer, ExplorerRow);
        
        LeftSplitter = new GridSplitter()
        {
            Width = 2,
            Background = Brushes.Gray,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        ContentGrid.Children.Add(LeftSplitter);
        Grid.SetColumn(LeftSplitter, ExplorerRow + 1);
        
        PreviewerControlTest = new MonoGameControlTest()
        {
            FallbackBackground = Brushes.DarkSlateGray,
            // Game = _uiPreviewer as Game
        };
        ContentGrid.Children.Add(PreviewerControlTest);
        Grid.SetColumn(PreviewerControlTest, PreviewerRow);
        
        RightSplitter = new GridSplitter()
        {
            Width = 2,
            Background = Brushes.Gray,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        ContentGrid.Children.Add(RightSplitter);
        Grid.SetColumn(RightSplitter, PreviewerRow + 1);
        
        Properties = new UiProperties();
        ContentGrid.Children.Add(Properties);
        Grid.SetColumn(Properties, PropertiesRow);
    }

    private void RegisterEvents()
    {
    }

}