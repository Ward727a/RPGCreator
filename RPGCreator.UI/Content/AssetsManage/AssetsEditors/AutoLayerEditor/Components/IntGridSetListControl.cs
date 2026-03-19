using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor.Components;

public class IntGridSetListItemControl : UserControl
{
    
    public event Action<IntGridTilesetDef>? OnSelected;
    
    public IntGridTilesetDef TilesetDefDef { get; private set; }
    public Grid? Body { get; private set; }
    
    public TextBlock? SetLabel { get; private set; }
    
    public IntGridSetListItemControl(IntGridTilesetDef tilesetDefDef)
    {
        TilesetDefDef = tilesetDefDef;
        CreateComponents();
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        Width = double.NaN;
        Content = Body;
        RegisterEvents();
    }
    
    private void CreateComponents()
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("auto, 5, Auto"),
            ColumnDefinitions = new ColumnDefinitions("auto, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Avalonia.Thickness(5),
            Background = Avalonia.Media.Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        SetLabel = new TextBlock()
        {
            Text = string.IsNullOrWhiteSpace(TilesetDefDef.Name) ? "Unnamed IntGrid Set" : TilesetDefDef.Name,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5),
        };
        Body.Children.Add(SetLabel);
        Grid.SetRow(SetLabel, 0);
        
        var divider = new Divider()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Body.Children.Add(divider);
        Grid.SetRow(divider, 2);
        Grid.SetColumnSpan(divider, 2);
    }
    
    private void RegisterEvents()
    {
        Body.PointerPressed += OnBodyPointerPressed;
    }

    private void OnBodyPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        OnSelected?.Invoke(TilesetDefDef);
    }
}

public class IntGridSetCreateModal : Window
{
    public event Action? OnCancelled;
    public event Action<IntGridTilesetDef>? OnIntGridSetCreated;

    public Grid? Body;
    
    public StackPanel? FormPanel;
    
    public TextBox? NameTextBox;
    public ComboBox? PackComboBox;
    
    public StackPanel? ButtonsPanel;
    public Button? CreateButton;
    public Button? CancelButton;
    
    
    public IntGridSetCreateModal()
    {
        Title = "Create New IntGrid Set";
        Width = 400;
        Height = 300;
        CreateComponents();
        RegisterEvents();
        Content = Body;
        
        // Implement the modal UI and logic here
    }
    
    private void CreateComponents()
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, auto"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Avalonia.Thickness(10)
        };
        
        FormPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Spacing = 10
        };
        Body.Children.Add(FormPanel);
        Grid.SetRow(FormPanel, 0);
        
        NameTextBox = new TextBox()
        {
            InnerLeftContent = "Name: ",
            Watermark = "Enter IntGrid Set Name",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        FormPanel.Children.Add(NameTextBox);
        
        PackComboBox = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            PlaceholderText = "Select Asset Pack"
        };
        FormPanel.Children.Add(PackComboBox);
        
        ButtonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10
        };
        Body.Children.Add(ButtonsPanel);
        Grid.SetRow(ButtonsPanel, 1);
        
        CancelButton = new Button()
        {
            Content = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        ButtonsPanel.Children.Add(CancelButton);
        
        CreateButton = new Button()
        {
            Content = "Create",
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        ButtonsPanel.Children.Add(CreateButton);
    }
    
    private void RegisterEvents()
    {
        CancelButton.Click += OnCancelButtonClick;
        CreateButton.Click += OnCreateButtonClick;
        
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RefreshPacksList();
    }
    
    private void RefreshPacksList()
    {
        PackComboBox.SelectedIndex = -1;
        PackComboBox.Items.Clear();
        var packs = EngineServices.AssetsManager.GetLoadedPacks();

        foreach (var pack in packs)
        {
            PackComboBox.Items.Add(new ComboBoxItem()
            {
                Content = pack.Name,
                Tag = pack
            });
        }
    }

    private void OnCancelButtonClick(object? sender, RoutedEventArgs e)
    {
        OnCancelled?.Invoke();
    }

    private void OnCreateButtonClick(object? sender, RoutedEventArgs e)
    {
        // Implement creation logic here
        var selectedPackItem = PackComboBox.SelectedItem as ComboBoxItem;
        if (selectedPackItem == null)
        {
            Logger.Warning("[IntGridSetCreateModal] No asset pack selected.");
            return;
        }
        var selectedPack = selectedPackItem.Tag as IAssetsPack;
        if (selectedPack == null)
        {
            Logger.Warning("[IntGridSetCreateModal] Selected asset pack is invalid.");
            return;
        }
        var intGridSetName = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(intGridSetName))
        {
            Logger.Warning("[IntGridSetCreateModal] IntGrid Set name is empty.");
            return;
        }
        var newIntGridSet = EngineServices.AssetsManager.CreateAsset<IntGridTilesetDef>();
        newIntGridSet.Name = intGridSetName;
        newIntGridSet.Pack = selectedPack;
        EngineServices.AssetsManager.GetDefaultPack().AddOrUpdateAsset(newIntGridSet);
        Logger.Info("[IntGridSetCreateModal] Created new IntGrid Set '{name}' in pack '{packName}'", intGridSetName, selectedPack.Name);
        
        OnIntGridSetCreated?.Invoke(newIntGridSet);
    }

}

public class IntGridSetListControl : UserControl
{
    
    private IntGridTilesetDef? _selectedTileset;
    
    public event Action<IntGridTilesetDef>? OnTilesetSelected;
    
    private IAssetScope _scope;
    
    public Grid? Body { get; private set; }
    public ScrollViewer? ListScroller { get; private set; }
    public StackPanel? ListBody { get; private set; }
    
    public StackPanel? MenuPanel { get; private set; }
    public Button? RemoveTilesetButton { get; private set; }
    public Button? EditTilesetButton { get; private set; }
    public Button? AddTilesetButton { get; private set; }
    
    public IntGridSetListControl(IAssetScope? scope = null)
    {
        
        if(scope == null)
            _scope = EngineServices.AssetsManager.CreateAssetScope();
        else
            _scope = scope;
        
        Width = double.NaN;
        
        CreateComponents();
        Content = Body;
        RegisterEvents();
    }
    
    private void CreateComponents()
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("50, 5, *"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        
        ListScroller = new ScrollViewer()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Body.Children.Add(ListScroller);
        Grid.SetRow(ListScroller, 2);
        
        ListBody = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Width = double.NaN,
            Spacing = 5
        };
        ListScroller.Content = ListBody;
        
        var divider = new Divider()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Body.Children.Add(divider);
        Grid.SetRow(divider, 1);
        
        MenuPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = double.NaN,
            Spacing = 5
        };
        Body.Children.Add(MenuPanel);
        Grid.SetRow(MenuPanel, 0);
        
        RemoveTilesetButton = new Button()
        {
            Content = "Remove IntGrid Set",
            HorizontalAlignment = HorizontalAlignment.Left,
            IsEnabled = false,
        };
        MenuPanel.Children.Add(RemoveTilesetButton);
        
        EditTilesetButton = new Button()
        {
            Content = "Edit IntGrid Set",
            HorizontalAlignment = HorizontalAlignment.Center,
            IsEnabled = false,
        };
        MenuPanel.Children.Add(EditTilesetButton);
        
        AddTilesetButton = new Button()
        {
            Content = "Add IntGrid Set",
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        MenuPanel.Children.Add(AddTilesetButton);
    }
    
    private void RegisterEvents()
    {
        RemoveTilesetButton.Click += (s, e) =>
        {
            if (_selectedTileset == null) return;
            Logger.Debug("[IntGridSetListControl] Removing IntGrid Set: {name}", _selectedTileset.Name);
            if (EngineServices.AssetsManager.TryResolveRegistry(_selectedTileset.GetType(), out var registry))
            {
                if (!registry.HasAsset(_selectedTileset))
                {
                    Logger.Error("[IntGridSetListControl] Failed to find IntGrid Set in registry: {name}", _selectedTileset.Name);
                    return;
                }
                if(_selectedTileset.Pack == null)
                {
                    Logger.Error("[IntGridSetListControl] IntGrid Set has no associated asset pack: {name}", _selectedTileset.Name);
                    return;
                }
                // Remove from pack
                _selectedTileset.Pack.RemoveAsset(_selectedTileset.Unique);
                // Remove from registry
                registry.UnregisterUntyped(_selectedTileset);
                RefreshList();
                RemoveTilesetButton.IsEnabled = false;
                EditTilesetButton.IsEnabled = false;
                _selectedTileset = null;
            }
            else
            {
                Logger.Error("[IntGridSetListControl] Failed to resolve registry for IntGrid Set: {name}", _selectedTileset.Name);
            }
        };
        AddTilesetButton.Click += OnAddTilesetButtonClick;
    }
    
    private void OnAddTilesetButtonClick(object? sender, RoutedEventArgs e)
    {
        Logger.Debug("[IntGridSetListControl] Add IntGrid Set button clicked.");
        var createModal = new IntGridSetCreateModal();
        
        createModal.OnCancelled += () =>
        {
            Logger.Debug("[IntGridSetListControl] IntGrid Set creation cancelled.");
            createModal.Close();
        };
        
        createModal.OnIntGridSetCreated += (newTileset) =>
        {
            Logger.Debug("[IntGridSetListControl] New IntGrid Set created: {name}", newTileset.Name);
            RefreshList();
            createModal.Close();
        };
        
        createModal.ShowDialog((Window?)VisualRoot);
    }
    
    private void RefreshList()
    {
        ListBody!.Children.Clear();
        
        var searchResults = EngineServices.AssetsManager.SearchAllPacks<IntGridTilesetDef>();
        
        foreach (var result in searchResults)
        {
            var tileset = _scope.Load<IntGridTilesetDef>(result.AssetId);
            
            var itemControl = new IntGridSetListItemControl(tileset);
            itemControl.OnSelected += (selectedTileset) =>
            {
                Logger.Debug("[IntGridSetListControl] Selected IntGrid Set: {name}", selectedTileset.Name);
                // Handle selection logic here
                OnTilesetSelected?.Invoke(selectedTileset);
                
                RemoveTilesetButton.IsEnabled = true;
                EditTilesetButton.IsEnabled = true;
                _selectedTileset = selectedTileset;
            };
            ListBody.Children.Add(itemControl);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RefreshList();
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _scope.Dispose();
    }
}