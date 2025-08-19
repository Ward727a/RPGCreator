using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Windows;

public class AssetExplorerDialog : Window
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties

    private AssetsManager _assets = EngineCore.Instance.Managers.Assets;
    public List<IHasUniqueId> AssetsList { get; private set; } = new List<IHasUniqueId>();
    #endregion
    
    #region Components

    protected Grid GridContent;

    protected Grid SearchPanel;
    protected TextBox SearchTextBox;
    protected Button SearchButton;
    
    protected ScrollBox ScrollBoxAssets;
    protected StackPanel AssetsListPanel;
    
    #endregion
    
    #region Constructors

    public AssetExplorerDialog()
    {
        CreateComponents();
        RegisterEvents();
        Title = "Asset explorer";
        Content = GridContent;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MinHeight = 300;
        MinWidth = 400;
        Height = 300;
        Width = 400;
        MaxHeight = 500;
        MaxWidth = 800;
        GetAssetsList();
    }

    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        GridContent = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *")
        };
        
        CreateSearchBar();
        CreateListComponents();
        
    }

    private void CreateSearchBar()
    {
        SearchPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
        };
        GridContent.Children.Add(SearchPanel);
        Grid.SetRow(SearchPanel, 0);
        
        SearchTextBox = new TextBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Watermark = "Search assets...",
            UseFloatingWatermark = true,
            Margin = new Avalonia.Thickness(5)
        };
        SearchPanel.Children.Add(SearchTextBox);
        
        SearchButton = new Button()
        {
            Content = "Search",
            Margin = new Avalonia.Thickness(5)
        };
        SearchPanel.Children.Add(SearchButton);
        Grid.SetColumn(SearchButton, 1);
    }

    private void CreateListComponents()
    {
        ScrollBoxAssets = new ScrollBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(5)
        };
        GridContent.Children.Add(ScrollBoxAssets);
        Grid.SetRow(ScrollBoxAssets, 1);
        
        AssetsListPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        ScrollBoxAssets.Content = AssetsListPanel;
    }

    private void RegisterEvents()
    {
        Loaded += OnLoaded;
    }

    private void GetAssetsList()
    {
        // This method should be overridden in derived classes to fetch the assets list.
        // For now, we just initialize an empty list.
        AssetsList = new List<IHasUniqueId>();
        
        // Start getting all assets from the different registry
        AddTilesets();
        AddMaps();
        AddStats();
        
        AssetsList.Sort((a,b) => a.Unique.CompareTo(b.Unique));
    }

    private void AddTilesets()
    {
        AddRegistry(_assets.TilesetRegistry);
    }
    
    private void AddMaps()
    {
        AddRegistry(_assets.MapRegistry);
    }
    private void AddStats()
    {
        AddRegistry(_assets.StatsRegistry);
    }
    
    private void AddRegistry<T>(IAssetRegistry<T> registry) where T : IHasUniqueId
    {
        var assets = registry.All();
        AssetsList.AddRange(assets as IEnumerable<IHasUniqueId>);
    }

    protected virtual void ReloadView()
    {
        AssetsListPanel.Children.Clear();
        
        foreach (var asset in AssetsList)
        {
            var assetButton = new Button
            {
                Content = $"{AssetsTypeMapping.GetAssetType(asset.GetType()).Key} : {asset.Urn.Name.Split('@').First()}",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            AssetsListPanel.Children.Add(assetButton);
        }
    }

    #endregion

    #region Events Handlers

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        ReloadView();
    }
    
    #endregion
}