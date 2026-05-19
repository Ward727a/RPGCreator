using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
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

    private void OnCancelButtonClick(object? sender, RoutedEventArgs e)
    {
        OnCancelled?.Invoke();
    }

    private void OnCreateButtonClick(object? sender, RoutedEventArgs e)
    {
        // Implement creation logic here

        var intGridSetName = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(intGridSetName))
        {
            Logger.Warning("[IntGridSetCreateModal] IntGrid Set name is empty.");
            return;
        }

        EngineServices.AssetsManager
            .Create<IntGridTilesetDef>("rpgc://assets/definitions/tilesets/int_grid_tileset").OnSuccess((
                intGrid) =>
            {
                intGrid.Name = intGridSetName;
                EngineServices.AssetsManager.Save(intGrid);
                Logger.Info("[IntGridSetCreateModal] Created new IntGrid Set '{name}'",
                    intGridSetName);
                OnIntGridSetCreated?.Invoke(intGrid);
            }).OnFailure((err) =>
            {
                Logger.Error("[IntGridSetCreateModal] Failed to create IntGrid Set: {err}", err);
            });
    }
}

public class IntGridSetListControl : UserControl
{
    private IntGridTilesetDef? _selectedTileset;

    public event Action<IntGridTilesetDef>? OnTilesetSelected;

    public Grid? Body { get; private set; }
    public ScrollViewer? ListScroller { get; private set; }
    public StackPanel? ListBody { get; private set; }

    public StackPanel? MenuPanel { get; private set; }
    public Button? RemoveTilesetButton { get; private set; }
    public Button? EditTilesetButton { get; private set; }
    public Button? AddTilesetButton { get; private set; }

    public IntGridSetListControl()
    {
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

            EngineServices.AssetsManager.Delete(_selectedTileset.Unique).OnSuccess(() =>
            {
                RefreshList();
                RemoveTilesetButton.IsEnabled = false;
                EditTilesetButton.IsEnabled = false;
                _selectedTileset = null;
            }).OnFailure((err) =>
            {
                Logger.Error("[IntGridSetListControl] Failed to remove IntGrid Set: {err}", err);
            });
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

        EngineServices.AssetsManager
            .GetAssetsOfClass(new URN("rpgc", "assets", "definitions", "tilesets", "int_grid_tileset")).OnSuccess(searchResult =>
            {
                foreach (var ulid in searchResult)
                {
                    EngineServices.AssetsManager.Load<IntGridTilesetDef>(ulid).OnSuccess((tilesetDef) =>
                    {
                        var itemControl = new IntGridSetListItemControl(tilesetDef);
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
                    });
                }
            });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RefreshList();
    }
}