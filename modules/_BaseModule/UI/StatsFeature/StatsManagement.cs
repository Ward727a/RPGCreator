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
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Extensions;

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

    private readonly AssetsManagerMenuContext _context;
    private ObservableCollection<string> _availableNames = new();
    private HashSet<BaseStatDefinition> _allStats = new();
    private ObservableCollection<BaseStatDefinition> _availableStats = new();

    private BaseStatDefinition? _selectedStat = null;
    
    #endregion
    
    public StatsManagement(AssetsManagerMenuContext context)
    {
        _context = context;
        CreateComponents();
        RegisterEvents();

        var config = new StatsManagerContext.Config()
        {
            GetStatsGrid = () => _statsGrid,
            GetSearchBar = () => _searchBar,
            GetGridScroller = () => _gridScroller,
            GetListBox = () => _listBox,
            GetButtonsPanel = () => _buttonsPanel,
            GetAddButton = () => _addButton,
            GetEditButton = () => _editButton,
            GetRemoveButton = () => _removeButton,
            ApplyFilters = ApplyFilter
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(new UIRegion("BaseModule.StatsManagement"), this, new StatsManagerContext(config));
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
            ItemsSource = _availableStats,
            ItemTemplate = new FuncDataTemplate<BaseStatDefinition>((definition, _) =>
            {
                if (definition == null) return null;
                return new TextBlock()
                {
                    Text = definition.DisplayName
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
        _context.OpenCustom(new StatEditor(_context));
    }

    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedStat == null) return;
        
        _context.OpenCustom(new StatEditor(_context, _selectedStat));
    }
    
    private async void RemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedStat == null) return;

        var result = await EditorUiServices.DialogService.ConfirmAsync("Are you sure?",
            $"This will permanently delete the stat definition ({_selectedStat.DisplayName}) and all references to it.\n" +
            $"This action cannot be undone!!!",
            confirmButtonText: "Delete", cancelButtonText: "Cancel");

        if (!result) return;
        
        var pack = EngineServices.AssetsManager.GetDefaultPack();
        pack.RemoveAsset(_selectedStat.Unique);
        _allStats.Remove(_selectedStat);
        ApplyFilter();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var values = EngineServices.AssetsManager.GetAssets<BaseStatDefinition>();
        foreach (var statDef in values)
        {
            _availableNames.Add(statDef.DisplayName);
            _allStats.Add(statDef);
            _availableStats.Add(statDef);
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
            if (_availableStats.Count == _allStats.Count) return;

            _availableStats.Clear();
            foreach (var stat in _allStats) _availableStats.Add(stat);
            return;
        }

        _availableStats.Clear();

        var query = searchQuery.ToLower();

        foreach (var stat in _allStats)
        {
            if (stat.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                _availableStats.Add(stat);
            }
        }
    }
    
    private void OnSelect(object? sender, SelectionChangedEventArgs e)
    {
        _selectedStat = _listBox.SelectedItem as BaseStatDefinition;
        UpdateButtonsState();
    }
    
    private void UpdateButtonsState()
    {
        bool hasSelection = _selectedStat != null;
        _editButton.IsEnabled = hasSelection;
        _removeButton.IsEnabled = hasSelection;
    }
}