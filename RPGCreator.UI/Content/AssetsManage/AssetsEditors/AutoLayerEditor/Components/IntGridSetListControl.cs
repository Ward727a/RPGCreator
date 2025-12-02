using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Types.Assets.Tilesets.IntGridTileset;
using Serilog;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor.Components;

public class IntGridSetListItemControl : UserControl
{
    
    public event Action<IntGridTileset>? OnSelected;
    
    public IntGridTileset TilesetDef { get; private set; }
    public Grid? Body { get; private set; }
    
    public TextBlock? SetLabel { get; private set; }
    
    public IntGridSetListItemControl(IntGridTileset tilesetDef)
    {
        TilesetDef = tilesetDef;
        CreateComponents();
        this.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        this.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
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
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Margin = new Avalonia.Thickness(5),
            Background = Avalonia.Media.Brushes.Transparent,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        SetLabel = new TextBlock()
        {
            Text = string.IsNullOrWhiteSpace(TilesetDef.Name) ? "Unnamed IntGrid Set" : TilesetDef.Name,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5),
        };
        Body.Children.Add(SetLabel);
        Grid.SetRow(SetLabel, 0);
        
        var divider = new Divider()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
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
        OnSelected?.Invoke(TilesetDef);
    }
}

public class IntGridSetCreateModal : Window
{
    private AssetScope _scope;
    public event Action? OnCancelled;
    public event Action<IntGridTileset>? OnIntGridSetCreated;

    public Grid? Body;
    
    public StackPanel? FormPanel;
    public TextBox? NameTextBox;
    
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
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Spacing = 10
        };
        Body.Children.Add(FormPanel);
        Grid.SetRow(FormPanel, 0);
        
        NameTextBox = new TextBox()
        {
            InnerLeftContent = "Name: ",
            Watermark = "Enter IntGrid Set Name",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };
        
        FormPanel.Children.Add(NameTextBox);
        
        ButtonsPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };
        Body.Children.Add(ButtonsPanel);
        Grid.SetRow(ButtonsPanel, 1);
        
        CancelButton = new Button()
        {
            Content = "Cancel",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        ButtonsPanel.Children.Add(CancelButton);
        
        CreateButton = new Button()
        {
            Content = "Create",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        };
        ButtonsPanel.Children.Add(CreateButton);
    }
    
    private void RegisterEvents()
    {
        CancelButton.Click += OnCancelButtonClick;
        CreateButton.Click += OnCreateButtonClick;
        
    }
    private void OnCancelButtonClick(object? sender, RoutedEventArgs e)
    {
        OnCancelled?.Invoke();
    }

    private void OnCreateButtonClick(object? sender, RoutedEventArgs e)
    {
        // Implement creation logic here
        Log.Debug("[IntGridSetCreateModal] Create button clicked.");
        Close();
    }

}

public class IntGridSetListControl : UserControl
{
    
    public event Action<IntGridTileset>? OnTilesetSelected;
    
    private AssetScope _scope;
    
    public Grid? Body { get; private set; }
    public ScrollViewer? ListScroller { get; private set; }
    public StackPanel? ListBody { get; private set; }
    
    public StackPanel? MenuPanel { get; private set; }
    public Button? AddTilesetButton { get; private set; }
    
    public IntGridSetListControl(AssetScope? scope = null)
    {
        
        if(scope == null)
            _scope = EngineCore.Instance.Managers.Assets.CreateAssetScope();
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
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };
        Body.Children.Add(ListScroller);
        Grid.SetRow(ListScroller, 2);
        
        ListBody = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Width = double.NaN,
            Spacing = 5
        };
        ListScroller.Content = ListBody;
        
        var divider = new Divider()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };
        Body.Children.Add(divider);
        Grid.SetRow(divider, 1);
        
        MenuPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        };
        Body.Children.Add(MenuPanel);
        Grid.SetRow(MenuPanel, 0);
        
        AddTilesetButton = new Button()
        {
            Content = "Add IntGrid Set",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        };
        MenuPanel.Children.Add(AddTilesetButton);
    }
    
    private void RegisterEvents()
    {
        AddTilesetButton.Click += OnAddTilesetButtonClick;
    }
    
    private void OnAddTilesetButtonClick(object? sender, RoutedEventArgs e)
    {
        Log.Debug("[IntGridSetListControl] Add IntGrid Set button clicked.");
        var createModal = new IntGridSetCreateModal();
        createModal.ShowDialog((Window?)this.VisualRoot);
    }
    
    private void RefreshList()
    {
        ListBody!.Children.Clear();
        
        var searchResults = EngineCore.Instance.Managers.Assets.SearchAllPacks<IntGridTileset>();
        
        foreach (var result in searchResults)
        {
            var tileset = _scope.Load<IntGridTileset>(result.AssetId);
            
            var itemControl = new IntGridSetListItemControl(tileset);
            itemControl.OnSelected += (selectedTileset) =>
            {
                Log.Debug("[IntGridSetListControl] Selected IntGrid Set: {name}", selectedTileset.Name);
                // Handle selection logic here
                OnTilesetSelected?.Invoke(selectedTileset);
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