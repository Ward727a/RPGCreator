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

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using RPGCreator.SDK;
using RPGCreator.SDK.EditorUI.Extensions;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;

namespace _BaseModule.UI.StatsFeature;

public class StatsManagement : UserControl
{
    
    #region Components
    private Grid _statsGrid = null!;

    private AutoCompleteBox _searchBar = null!;
    
    private ScrollViewer _gridScroller = null!;
    private ListBox _listBox = null!;
    
    private StackPanel _buttonsPanel = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _removeButton = null!;
    #endregion
    
    #region Properties
    
    private ObservableCollection<string> _availableNames = new();
    
    #endregion
    
    public StatsManagement()
    {
        CreateComponents();
        RegisterEvents();

        var config = new StatsUiContext.Config()
        {
            GetStatsGrid = () => _statsGrid,
        };
        
        UiServices.ExtensionManager.ApplyExtensions(new UIRegion("BaseModule.StatsManagement"), this, new StatsUiContext(config));
    }

    private void CreateComponents()
    {

        _statsGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *, Auto"),
            RowSpacing = 10
        };
        this.Content = _statsGrid;

        _searchBar = new AutoCompleteBox()
        {
            Watermark = "Search stats...",
            ItemsSource = _availableNames,
            FilterMode = AutoCompleteFilterMode.Contains
        };
        _statsGrid.Children.Add(_searchBar);
        Grid.SetRow(_searchBar, 0);
        
        _gridScroller = new ScrollViewer()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        _statsGrid.Children.Add(_gridScroller);
        Grid.SetRow(_gridScroller, 1);

        _listBox = new ListBox();
        _gridScroller.Content = _listBox;
        
        _buttonsPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        };
        _statsGrid.Children.Add(_buttonsPanel);
        Grid.SetRow(_buttonsPanel, 2);
        
        Thickness buttonMargin = new Thickness(5, 0, 0, 0);
        
        _addButton = new Button()
        {
            Content = "Add",
            Margin = buttonMargin
        };
        _buttonsPanel.Children.Add(_addButton);
        
        _editButton = new Button()
        {
            Content = "Edit",
            Margin = buttonMargin
        };
        _buttonsPanel.Children.Add(_editButton);
        
        _removeButton = new Button()
        {
            Content = "Remove",
            Margin = buttonMargin
        };
        _buttonsPanel.Children.Add(_removeButton);
        
    }

    private void RegisterEvents()
    {
        Loaded += StatsManagement_Loaded;
    }

    private void StatsManagement_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!EngineServices.GlobalPathData.TryGetPaths(Features.Entity.StatsFeature.StatsTag, out var paths)) return;
        foreach (var path in paths)
        {
            _listBox.Items.Add(path);
        }
    }
}