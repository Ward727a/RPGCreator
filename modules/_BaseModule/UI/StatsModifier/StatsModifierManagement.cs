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
using _BaseModule.AssetDefinitions.BaseStats;
using _BaseModule.UI.StatsFeature;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Extensions;
using RPGCreator.UI.Contexts;

namespace _BaseModule.UI.StatsModifier;

public class StatsModifierManagement : UserControl
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

    private readonly AssetsManagerMenuContext _context;
    private ObservableCollection<string> _availableNames = new();
    private HashSet<StatModifierDefinition> _allModifiers = new();
    private ObservableCollection<StatModifierDefinition> _availableModifiers = new();

    private StatModifierDefinition? _selectedModifier = null;
    
    #endregion
    
    public StatsModifierManagement(AssetsManagerMenuContext menuContext)
    {
        _context = menuContext;
        CreateComponents();
        RegisterEvents();
    }

    private void CreateComponents()
    {
        _statsGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *, Auto"),
            RowSpacing = 10,
            Margin = new Thickness(10)
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

        _listBox = new ListBox()
        {
            ItemsSource = _availableModifiers,
            ItemTemplate = new FuncDataTemplate<StatModifierDefinition>((definition, _) =>
            {
                if (definition == null) return null;
                return new TextBlock()
                {
                    Text = definition.DisplayName,
                    TextWrapping = TextWrapping.Wrap
                };
            })
        };
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
            Margin = buttonMargin,
            IsEnabled = false
        };
        _buttonsPanel.Children.Add(_editButton);
        
        _removeButton = new Button()
        {
            Content = "Remove",
            Margin = buttonMargin,
            IsEnabled = false
        };
        _buttonsPanel.Children.Add(_removeButton);
        
    }

    private void RegisterEvents()
    {
        Loaded += OnLoaded;
        
        _searchBar.TextChanged += OnFilter;
        _listBox.SelectionChanged += OnSelect;
        
        _addButton.Click += AddButton_Click;
        _editButton.Click += EditButton_Click;
        _removeButton.Click += RemoveButton_Click;
    }

    private void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        _context.OpenCustom(new StatModifierEditor(_context));
    }

    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedModifier == null) return;
        
        _context.OpenCustom(new StatModifierEditor(_context, _selectedModifier));
    }
    
    private async void RemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedModifier == null) return;

        var result = await EditorUiServices.DialogService.ConfirmAsync("Are you sure?",
            $"This will permanently delete the stat modifier ({_selectedModifier.DisplayName}) and all references to it.\n" +
            $"This action cannot be undone!!!",
            confirmButtonText: "Delete", cancelButtonText: "Cancel");

        if (!result) return;
        
        var pack = EngineServices.AssetsManager.GetDefaultPack();
        pack.RemoveAsset(_selectedModifier.Unique);
        _allModifiers.Remove(_selectedModifier);
        ApplyFilter();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var values = EngineServices.AssetsManager.GetAssets<StatModifierDefinition>();
        foreach (var modifierDef in values)
        {
            _availableNames.Add(modifierDef.DisplayName);
            _allModifiers.Add(modifierDef);
            _availableModifiers.Add(modifierDef);
        }
    }

    private void OnFilter(object? sender, TextChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(ApplyFilter, DispatcherPriority.Normal);
    }

    private void ApplyFilter()
    {
        var searchQuery = _searchBar.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(searchQuery))
        {
            if (_availableModifiers.Count == _allModifiers.Count) return;

            _availableModifiers.Clear();
            foreach (var stat in _allModifiers) _availableModifiers.Add(stat);
            return;
        }

        _availableModifiers.Clear();

        var query = searchQuery.ToLower();

        foreach (var stat in _allModifiers)
        {
            if (stat.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                _availableModifiers.Add(stat);
            }
        }
    }
    
    private void OnSelect(object? sender, SelectionChangedEventArgs e)
    {
        _selectedModifier = _listBox.SelectedItem as StatModifierDefinition;
        UpdateButtonsState();
    }
    
    private void UpdateButtonsState()
    {
        bool hasSelection = _selectedModifier != null;
        _editButton.IsEnabled = hasSelection;
        _removeButton.IsEnabled = hasSelection;
    }
}