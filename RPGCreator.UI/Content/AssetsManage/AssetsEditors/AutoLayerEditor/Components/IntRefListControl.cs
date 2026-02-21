using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Records;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Common;
using RPGCreator.UI.Common.CustomBrush;
using RPGCreator.UI.Common.TilesetsCommonComponents;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;
using RPGCreator.UI.Extensions;
using Size = System.Drawing.Size;

// ReSharper disable MemberCanBePrivate.Global

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor.Components;


public class IntRefListMenu : Grid
{
    public Action? OnCreateIntRef;
    public Action? OnSaveTileset;
    
    public Button? CreateButton;
    public Button? SaveButton;
    
    public IntRefListMenu()
    {
        RowDefinitions = new RowDefinitions("Auto");
        ColumnDefinitions = new ColumnDefinitions("*, 5, *");
        HorizontalAlignment = HorizontalAlignment.Stretch;
        CreateComponents();
        RegisterEvents();
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefListMenu, this);
    }
    
    private void CreateComponents()
    {
        CreateButton = new Button()
        {
            Content = "Create Group",
        };
        Children.Add(CreateButton);
        
        SaveButton = new Button()
        {
            Content = "Save",
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        Children.Add(SaveButton);
        Grid.SetColumn(SaveButton, 2);
    }
    
    private void RegisterEvents()
    {
        if (CreateButton != null)
        {
            CreateButton.Click += (_, _) => OnCreateIntRef?.Invoke();
        }
        
        if (SaveButton != null)
        {
            SaveButton.Click += (_, _) => OnSaveTileset?.Invoke();
        }
    }
}

public class IntRefListCreateModal : Window
{

    public class IntRefSelectDefaultTile : Window
    {
        private IAssetScope _scope;
        private ITileDef? _selectedTile = null;
        
        public Action<ITileDef?>? OnTileSelected;
        
        Grid? Body;
        TilesetExplorer? TilesetExplorer;
        StackPanel? ButtonsPanel;
        Button? ConfirmButton;
        Button? CancelButton;
        public IntRefSelectDefaultTile()
        {
            
            _scope = EngineServices.AssetsManager.CreateAssetScope();
            
            Title = "Select Default Tile";
            SizeToContent = SizeToContent.WidthAndHeight;
            CanResize = false;
            CanMaximize = false;
            
            CreateComponents();
            RegisterEvents();
            
        }

        private void CreateComponents()
        {
            Body = new Grid()
            {
                RowDefinitions = new RowDefinitions("Auto, *"),
                ColumnDefinitions = new ColumnDefinitions("*"),
                Margin = new Thickness(10),
            };
            Content = Body;
            
            TilesetExplorer = new TilesetExplorer(_scope, tilesetType: TilesetExplorer.TilesetType.NonAutotileOnly);
            Body.Children.Add(TilesetExplorer);
            Grid.SetRow(TilesetExplorer, 0);
            
            ButtonsPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            Body.Children.Add(ButtonsPanel);
            Grid.SetRow(ButtonsPanel, 1);
            ConfirmButton = new Button()
            {
                Content = "Confirm",
                Margin = new Thickness(5, 0, 0, 0),
            };
            ButtonsPanel.Children.Add(ConfirmButton);
            CancelButton = new Button()
            {
                Content = "Cancel",
                Margin = new Thickness(5, 0, 0, 0),
            };
            ButtonsPanel.Children.Add(CancelButton);
        }

        private void RegisterEvents()
        {
            TilesetExplorer?.TileSelected += (obj) =>
            {
                if(obj is ITileDef tile)
                    _selectedTile = tile;
            };
            ConfirmButton?.Click += (_, _) =>
            {
                OnTileSelected?.Invoke(_selectedTile);
            };
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            _scope.Dispose();
        }
    }
    
    private readonly IAssetScope _scope;
    
    [ExposeEventToPlugin("AutoLayerEditor.IntRefList.CreateModal")]
    public event Action? OnCreateIntRefConfirmed;
    [ExposeEventToPlugin("AutoLayerEditor.IntRefList.CreateModal")]
    public event Action? OnCreateIntRefCancelled;
    
    public IntGridValueRef? FromRef;
    public ITileDef? SelectedDefaultTile;
    public bool IsEdit => FromRef != null;
    
    public StackPanel? Body;
    
    public TextBox? NameInput;
    public ColorPicker? ColorInput;
    public StackPanel? DefaultTilePanel;
    public Button? SelectDefaultTileButton;
    public Image? DefaultTileImage;
    
    public StackPanel? ButtonsPanel;
    public Button? ConfirmButton;
    public Button? CancelButton;
    
    public IntRefListCreateModal(IntGridValueRef? @ref = null) 
    {
        _scope = EngineServices.AssetsManager.CreateAssetScope();
        FromRef = @ref;
        if(IsEdit)
        {
            Title = "Edit IntRef";
        }
        else
        {
            Title = "Create IntRef";
        }
        Width = 300;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        CanMaximize = false;
        
        CreateComponents();
        RegisterEvents();

        AutoLayerEditorIntRefListCreateModalContext.Config config = new()
        {
            BakeIntRef = BakeIntRef,
            AddOnCreateIntRefCancelled = (handler) => OnCreateIntRefCancelled += handler,
            RemoveOnCreateIntRefCancelled = (handler) => OnCreateIntRefCancelled -= handler,
            AddOnCreateIntRefConfirmed = (handler) => OnCreateIntRefConfirmed += handler,
            RemoveOnCreateIntRefConfirmed = (handler) => OnCreateIntRefConfirmed -= handler
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefListCreateModal, this, config);
    }
    
    private void CreateComponents()
    {
        Body = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Margin = new Thickness(10),
        };
        Content = Body;
        
        NameInput = new TextBox()
        {
            Watermark = "IntRef Name",
            Text = FromRef?.Name ?? string.Empty,
            Margin = new Thickness(0, 0, 0, 10),
        };
        Body.Children.Add(NameInput);
        
        ColorInput = new ColorPicker()
        {
            Color = FromRef?.Color.ToAvalonia() ?? Avalonia.Media.Colors.White,
            ColorModel = ColorModel.Rgba,
            Palette = new MaterialHalfColorPalette(),
            Margin = new Thickness(0, 0, 0, 10),
        };
        Body.Children.Add(ColorInput);

        DefaultTilePanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10),
        };
        Body.Children.Add(DefaultTilePanel);
        
        SelectDefaultTileButton = new Button()
        {
            Content = "Select Default Tile",
        };
        DefaultTilePanel.Children.Add(SelectDefaultTileButton);
        
        DefaultTileImage = new Image()
        {
            Width = 32,
            Height = 32,
            Margin = new Thickness(10, 0, 0, 0),
        };
        DefaultTilePanel.Children.Add(DefaultTileImage);
        
        if(FromRef == null || FromRef.DefaultTileData.TilesetId == Ulid.Empty)
        {
            DefaultTileImage.Source = EditorAssets.FallbackImage;
        }
        else
        {
            var tileset = _scope.Load<BaseTilesetDef>(FromRef.DefaultTileData.TilesetId);

            if (tileset != null)
            {
                var bitmap = EngineServices.ResourcesService.Load<Bitmap>(tileset.ImagePath);

                if (bitmap != null)
                {
                    var rect = new PixelRect(
                        (int)FromRef.DefaultTileData.TilePosition.X,
                        (int)FromRef.DefaultTileData.TilePosition.Y,
                        tileset.TileWidth,
                        tileset.TileHeight
                    );

                    DefaultTileImage.Source = new CroppedBitmap(bitmap, rect);
                }
                else
                {
                    // Fallback image si le chargement échoue
                    DefaultTileImage.Source = EditorAssets.FallbackImage;
                }
            }
        }
        
        ButtonsPanel = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
        };
        Body.Children.Add(ButtonsPanel);
        
        ConfirmButton = new Button()
        {
            Content = IsEdit ? "Save" : "Create",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(ConfirmButton);
        
        CancelButton = new Button()
        {
            Content = "Cancel",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(CancelButton);
    }
    
    private void RegisterEvents()
    {
        if (ConfirmButton != null)
        {
            ConfirmButton.Click += (_, _) => OnCreateIntRefConfirmed?.Invoke();
        }
        
        if (CancelButton != null)
        {
            CancelButton.Click += (_, _) => OnCreateIntRefCancelled?.Invoke();
        }

        if (SelectDefaultTileButton != null)
        {
            SelectDefaultTileButton.Click += (_, _) => 
            {
                var tileModal = new IntRefSelectDefaultTile();
                tileModal.OnTileSelected += (tile) =>
                {
                    SelectedDefaultTile = tile;
                    if (tile != null)
                    {
                        var tileset = tile.TilesetDef;
                        var tilesetImage = EngineServices.ResourcesService.Load<Bitmap>(tileset.ImagePath);
                        var croppedImage = new CroppedBitmap(
                            tilesetImage,
                            new PixelRect(
                                (int)tile.PositionInTileset.X,
                                (int)tile.PositionInTileset.Y,
                                (int)tile.SizeInTileset.Width,
                                (int)tile.SizeInTileset.Height
                            )
                        );
                        DefaultTileImage!.Source = croppedImage;
                    }
                    else
                    {
                        DefaultTileImage!.Source = EditorAssets.FallbackImage;
                    }
                    tileModal.Close();
                };
                tileModal.ShowDialog((Window?)this.GetVisualRoot()!);
            };
        }
    }

    [ExposeToPlugin("AutoLayerEditor.IntRefList.CreateModal")]
    public IntGridValueRef? BakeIntRef()
    {
        if (string.IsNullOrWhiteSpace(NameInput?.Text))
            return null;

        IntGridValueRef intRef;

        if (IsEdit)
            intRef = FromRef;
        else
            intRef = new IntGridValueRef();

        intRef.Name = NameInput.Text;
        intRef.Color = intRef.Color.FromAvalonia(ColorInput?.Color ?? Avalonia.Media.Colors.White);
        
        if (SelectedDefaultTile != null)
        {
            intRef.DefaultTileData = TileData.FromTileDef(SelectedDefaultTile);
        }
        
        return intRef;
    }
}

public class AutoLayerRuleSelectOutputTileModal : Window
{
    
    public event Action<CroppedBitmap, ITileDef>? OnCreateOutputTileConfirmed;
    public event Action<int>? OnSelectTilesetChanged; 
    
    private ITileDef? _selectedTile = null;
    
    public Grid? Body;
    public TilesetExplorer? TilesetExplorer;
    public StackPanel? ButtonsPanel;
    public Button? ConfirmButton;
    public Button? CancelButton;

    private IAssetScope _scope = EngineServices.AssetsManager.CreateAssetScope();
    
    public AutoLayerRuleSelectOutputTileModal(int baseTilesetIndex = -1)
    {
        this.SizeToContent = SizeToContent.WidthAndHeight;
        CreateComponents(baseTilesetIndex);
        RegisterEvents();
    }
    
    private void CreateComponents(int baseTilesetIndex = -1)
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, 50"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Thickness(10),
        };
        Content = Body;
        
        TilesetExplorer = new TilesetExplorer(_scope, canvasSize: new Size(1024, 640), baseSelectedIndex:baseTilesetIndex);
        Body.Children.Add(TilesetExplorer);
        Grid.SetRow(TilesetExplorer, 0);
        
        ButtonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        Body.Children.Add(ButtonsPanel);
        Grid.SetRow(ButtonsPanel, 1);
        ConfirmButton = new Button()
        {
            Content = "Confirm",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(ConfirmButton);
        CancelButton = new Button()
        {
            Content = "Cancel",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(CancelButton);
    }
    
    private void RegisterEvents()
    {
        if (TilesetExplorer == null || ConfirmButton == null || CancelButton == null)
            return;
        
        TilesetExplorer.TileSelected += (obj) =>
        {
            if(obj is ITileDef tile)
                _selectedTile = tile;
            else if (obj is IntGridValueRef intRef)
            {
                
            }
        };
        TilesetExplorer.TilesetChanged += (index) =>
        {
            OnSelectTilesetChanged?.Invoke(index);
        };
        
        ConfirmButton.Click += (_, _) =>
        {
            if (_selectedTile == null) return;
            
            Bitmap? tilesetImage = null;
            CroppedBitmap? outputTile = null;

            var tileset = _selectedTile.TilesetDef;
            tilesetImage = EngineServices.ResourcesService.Load<Bitmap>(tileset.ImagePath);
            
            if (tilesetImage == null)
                return;
                
            outputTile = new CroppedBitmap(
                tilesetImage,
                new PixelRect(
                    (int)_selectedTile.PositionInTileset.X,
                    (int)_selectedTile.PositionInTileset.Y,
                    (int)_selectedTile.SizeInTileset.Width,
                    (int)_selectedTile.SizeInTileset.Height
                )
            );
            OnCreateOutputTileConfirmed?.Invoke(outputTile, _selectedTile);
        };
    }
}

public class AutoLayerRuleCreateModal : Window
{
    private class RemoveTileCmd : ICommand
    {
        Border _tileBorder;
        TileData _tileData;
        
        AutoLayerRule _rule;
        StackPanel? _outputTilesPanel;
        
        public RemoveTileCmd(Border tileBorder, TileData tileData, 
            AutoLayerRuleCreateModal parent)
        {
            _tileBorder = tileBorder;
            _tileData = tileData;
            _rule = parent.Rule;
            _outputTilesPanel = parent.OutputTilesPanel;
        }
        
        public void Execute()
        {
            _rule.OutputTiles.Remove(_tileData);
            _outputTilesPanel?.Children.Remove(_tileBorder);
        }

        public void Undo()
        {
            _rule.OutputTiles.Add(_tileData);
            _outputTilesPanel?.Children.Add(_tileBorder);
        }

        public string Name { get; } = "Remove Output Tile";
    }
    
    private class AddTileCmd : ICommand
    {
        Border _tileBorder;
        TileData _tileData;
        CroppedBitmap _outputTile;
        ITileDef _selectedTile;
        
        AutoLayerRule _rule;
        StackPanel? _outputTilesPanel;
        
        Action<TileData, Border>? _requestRemoveTile;
        
        public AddTileCmd(
            CroppedBitmap outputTile, 
            ITileDef selectedTile,
            AutoLayerRuleCreateModal parent,
            Action<TileData, Border> requestRemoveTile)
        {
            _outputTile = outputTile;
            _selectedTile = selectedTile;
            _rule = parent.Rule;
            _outputTilesPanel = parent.OutputTilesPanel;
            _tileData = TileData.FromTileDef(_selectedTile);
            _requestRemoveTile = requestRemoveTile;
        }
        
        public void Execute()
        {
            _tileBorder = new Border()
            {
                Width = 64,
                Height = 64,
                BorderBrush = Brushes.Gray,
                Background = new SolidColorBrush(new Color(255, 22, 22, 26)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5),
                CornerRadius = new CornerRadius(4),
                Cursor = new Cursor(StandardCursorType.Hand)
            };
                
            var tileImage = new Image()
            {
                Source = _outputTile,
                Width = 56,
                Height = 56,
            };
            _tileBorder.Child = tileImage;
            RenderOptions.SetBitmapInterpolationMode(tileImage, BitmapInterpolationMode.None);
            _outputTilesPanel?.Children.Add(_tileBorder);

            _tileBorder.PointerPressed += (_, _) => _requestRemoveTile?.Invoke(_tileData, _tileBorder);
            ToolTip.SetTip(_tileBorder, "Click to remove this tile from output tiles.");
            
            _rule.OutputTiles.Add(_tileData);
        }

        public void Undo()
        {
            _rule.OutputTiles.Remove(_tileData);
            _outputTilesPanel?.Children.Remove(_tileBorder);
        }
        
        public string Name { get; } = "Add Output Tile";
    }
    
    public CommandManager CommandManager;
    
    private event Action<IntGridValueRef?>? OnSelectRefChanged;
    public event Action<AutoLayerRule>? OnCreateRuleConfirmed;
    
    public AutoLayerRule Rule { get; } = new AutoLayerRule();
    
    public int SelectedTilesetIndex { get; private set; } = -1;
    
    public IntRefContext Context { get; private init; }
    
    public readonly IntGridValueRef FromRef;
    private IntGridValueRef? _selectedRef = null;
    public IntGridValueRef? SelectedRef
    {
        get => _selectedRef;
        set
        {
            _selectedRef = value;
            OnSelectRefChanged?.Invoke(_selectedRef);
        }
    }
    
    public Grid? Body;
    
    public WindowNotificationManager? NotificationManager;
    
    public Grid? OutputTilesGrid;
    public TextBlock? OutputTilesLabel;
    public Button? OutputTilesButton;
    public Border? OutputTilesScrollBorder;
    public ScrollViewer? OutputTilesScrollViewer;
    public StackPanel? OutputTilesPanel;
    public WrapPanel? TargetGroupPanel;
    public Grid? ConditionsPanel;
    public Grid? BottomGrid;
    public StackPanel? ParemetersPanel;
    public CheckBox? FlipXParameter;
    public CheckBox? FlipYParameter;
    public NumericIntUpDown? ChanceParameter;
    public StackPanel? ButtonsPanel;
    public Button? CreateRuleButton;
    public Button? CancelButton;
    public Button? TestUndoButton;
    public Button? TestRedoButton;

    public AutoLayerRuleCreateModal(IntGridValueRef @ref, IntRefContext context)
    {
        FromRef = @ref;
        Context = context;
        CommandManager = new CommandManager();
        CreateComponents();
        RegisterEvents();
        PopulateTargetGroupPanel();
        PopulateConditionsPanel();
        Content = Body;
    }

    private void CreateComponents()
    {
        NotificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
        };
        
        int firstRow = 0;
        int secondRow = 2;
        int thirdRow = 4;
        
        int firstCol = 0;
        int secondCol = 2;
        
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, 5, *, 5, 50"),
            ColumnDefinitions = new ColumnDefinitions("310, 5, 1.4*"),
            Margin = new Thickness(10),
        };

        var roundBorder = new Border()
        {
            CornerRadius = new CornerRadius(4),
            BorderBrush = Brushes.DimGray,
            BorderThickness = new Thickness(1),
        };
        Body.Children.Add(roundBorder);
        Grid.SetRow(roundBorder, firstRow);
        Grid.SetColumn(roundBorder, firstCol);
        Grid.SetColumnSpan(roundBorder, 3);
        OutputTilesGrid = new Grid()
        {
            ColumnDefinitions =  new ColumnDefinitions("Auto, 5, *"),
            RowDefinitions = new RowDefinitions("*, 5, *"),
            Height = 80,
            Margin = new Thickness(5),
        };
        roundBorder.Child = OutputTilesGrid;
        
        OutputTilesLabel = new TextBlock()
        {
            Text = "Output Tiles:",
            FontWeight = FontWeight.Bold,
            VerticalAlignment = VerticalAlignment.Center,
        };
        OutputTilesGrid.Children.Add(OutputTilesLabel);
        Grid.SetColumn(OutputTilesLabel, 0);
        
        OutputTilesButton = new Button()
        {
            Content = "Add Tile",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        OutputTilesGrid.Children.Add(OutputTilesButton);
        Grid.SetColumn(OutputTilesButton, 0);
        Grid.SetRow(OutputTilesButton, 2);
        
        OutputTilesScrollBorder = new Border()
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Background = HatchBrush.CreateHatchBrush(new Color(100, 120, 120, 120), angle: -45, thickness: 1D),
        };
        OutputTilesGrid.Children.Add(OutputTilesScrollBorder);
        Grid.SetColumn(OutputTilesScrollBorder, 2);
        Grid.SetRowSpan(OutputTilesScrollBorder, 3);
        
        OutputTilesScrollViewer = new ScrollViewer()
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };
        OutputTilesScrollBorder.Child = OutputTilesScrollViewer;
        
        OutputTilesPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            ClipToBounds = true,
        };
        OutputTilesScrollViewer.Content = OutputTilesPanel;
        
        TargetGroupPanel = new WrapPanel()
        {
            Orientation = Orientation.Horizontal,
            ItemHeight = 100,
            ItemWidth = 100,
            ItemsAlignment = WrapPanelItemsAlignment.Start,
            ItemSpacing = 5,
            LineSpacing = 5,
        };
        var scrollViewer = new ScrollViewer()
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = TargetGroupPanel
        };
        Body.Children.Add(scrollViewer);
        Grid.SetRow(scrollViewer, secondRow);
        Grid.SetColumn(scrollViewer, firstCol);
        
        ConditionsPanel = new Grid()
        {
            RowDefinitions = new RowDefinitions("*,100, 5, 100, 5, 100,*"),
            ColumnDefinitions = new ColumnDefinitions("*,100, 5, 100, 5, 100,*"),
            Background = CheckerBoardBrush.CreateCheckerBoardBrush(new Color(
                5, 200, 200, 200
                ), Colors.Transparent, 50),
        };
        Body.Children.Add(ConditionsPanel);
        Grid.SetRow(ConditionsPanel, secondRow);
        Grid.SetColumn(ConditionsPanel, secondCol);
        
        BottomGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, 5, Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Body.Children.Add(BottomGrid);
        Grid.SetRow(BottomGrid, thirdRow);
        Grid.SetColumn(BottomGrid, firstCol);
        Grid.SetColumnSpan(BottomGrid, 3);
        
        ParemetersPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5
        };
        BottomGrid.Children.Add(ParemetersPanel);
        
        FlipXParameter = new CheckBox()
        {
            Content = "Flip X",
            VerticalAlignment = VerticalAlignment.Center
        };
        ParemetersPanel.Children.Add(FlipXParameter);
        
        FlipYParameter = new CheckBox()
        {
            Content = "Flip Y",
            VerticalAlignment = VerticalAlignment.Center
        };
        ParemetersPanel.Children.Add(FlipYParameter);
        
        ChanceParameter = new NumericIntUpDown()
        {
            Minimum = 0,
            Maximum = 100,
            Value = 100,
            NumberFormat = NumberFormatInfo.InvariantInfo,
            InnerRightContent = "%",
            InnerLeftContent = "Chance:",
            VerticalAlignment = VerticalAlignment.Center
        };
        ParemetersPanel.Children.Add(ChanceParameter);
        
        ButtonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        BottomGrid.Children.Add(ButtonsPanel);
        Grid.SetColumn(ButtonsPanel, 2);
        
        CancelButton = new Button()
        {
            Content = "Cancel",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(CancelButton);
        
        CreateRuleButton = new Button()
        {
            Content = "Create Rule",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(CreateRuleButton);
        
        TestUndoButton = new Button()
        {
            Content = "Undo",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(TestUndoButton);
        TestUndoButton.Click += (_, _) => CommandManager.UndoLastCommand();
        TestRedoButton = new Button()
        {
            Content = "Redo",
            Margin = new Thickness(5, 0, 0, 0),
        };
        ButtonsPanel.Children.Add(TestRedoButton);
        TestRedoButton.Click += (_, _) => CommandManager.RedoLastCommand();
        
    }
    private void RegisterEvents()
    {
        if (OutputTilesButton == null || CreateRuleButton == null || CancelButton == null)
            return;
        
        OutputTilesButton.Click += (_, _) =>
        {
            var tileModal = new AutoLayerRuleSelectOutputTileModal(SelectedTilesetIndex);
            tileModal.OnCreateOutputTileConfirmed += (outputTile, selectedTile) =>
            {
                var cmd = new AddTileCmd(
                    outputTile,
                    selectedTile,
                    this,
                    (tileData, tileBorder) =>
                    {
                        var removeCmd = new RemoveTileCmd(tileBorder, tileData, this);
                        CommandManager.ExecuteCommand(removeCmd);
                    }
                );
                CommandManager.ExecuteCommand(cmd);
                
                tileModal.Close();
                NotificationManager?.Show(
                    new Notification("Tile Added", "The output tile has been added successfully."),
                    type: NotificationType.Success,
                    showClose: false,
                    showIcon:true
                    );
            };
            
            tileModal.OnSelectTilesetChanged += (index) =>
            {
                SelectedTilesetIndex = index;
            };
            tileModal.ShowDialog((Window?)this.GetVisualRoot()!);
        };
        
        FlipXParameter.IsCheckedChanged += (_, _) =>
        {
            Rule.FlipX = FlipXParameter.IsChecked ?? false;
        };
        FlipYParameter.IsCheckedChanged += (_, _) =>
        {
            Rule.FlipY = FlipYParameter.IsChecked ?? false;
        };
        ChanceParameter.ValueChanged += (_, _) =>
        {
            Rule.Chance = ChanceParameter.Value ?? 100;
        };

        CreateRuleButton.Click += (_, _) =>
        {
            if (Rule.OutputTiles.Count == 0)
            {
                NotificationManager?.Show(
                    new Notification("No Output Tiles",
                        "Please add at least one output tile before creating the rule."),
                    type: NotificationType.Error,
                    showClose: false,
                    showIcon: true
                );
                return;
            }
            
            OnCreateRuleConfirmed?.Invoke(Rule);
        };
        
        CommandManager.StateChanged += () =>
        {
            TestUndoButton.IsEnabled = CommandManager.CanUndo;
            ToolTip.SetTip(TestUndoButton, $"Undo: {CommandManager.GetUndoCommandName()}");
            ToolTip.SetShowOnDisabled(TestUndoButton, true);
            TestRedoButton.IsEnabled = CommandManager.CanRedo;
            ToolTip.SetTip(TestRedoButton, $"Redo: {CommandManager.GetRedoCommandName()}");
            ToolTip.SetShowOnDisabled(TestRedoButton, true);
        };
    }

    public void PopulateTargetGroupPanel()
    {
        foreach (var intRef in Context.IntRefs)
        {
            var text = new TextBlock()
            {
                Text = $"{intRef.Name} ({intRef.Value})",
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var button = new Border()
            {
                Child = text,
                Background = new SolidColorBrush(intRef.Color.ToAvalonia()),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            
            button.PointerPressed += (_, _) =>
            {
                SelectedRef = intRef;
            };
            
            // intRef.Color but lighter for text if intRef.Color is dark, darker if intRef.Color is light
            var brightness = (intRef.Color.R * 0.299 + intRef.Color.G * 0.587 + intRef.Color.B * 0.114) / 255;
            if (brightness < 0.5)
            {
                text.Foreground = new SolidColorBrush(new Color(
                    (byte)255,
                    (byte)Math.Min(255, intRef.Color.R + 150),
                    (byte)Math.Min(255, intRef.Color.G + 150),
                    (byte)Math.Min(255, intRef.Color.B + 150)
                ));
            }
            else
            {
                text.Foreground = new SolidColorBrush(new Color(
                    (byte)255,
                    (byte)Math.Max(0, intRef.Color.R - 150),
                    (byte)Math.Max(0, intRef.Color.G - 150),
                    (byte)Math.Max(0, intRef.Color.B - 150)
                ));
            }
            
            TargetGroupPanel?.Children.Add(button);
        }

    }

    private class SelectConditionPanel : ICommand
    {
        private readonly AutoLayerRuleCreateModal _parent;
        private readonly Action _deselectAction;
        private readonly Action<bool> _selectAction;
        private readonly Action<int, int, PatternCondition, bool> _setPatternAction;
        private int _index;
        
        private PatternCondition _initialCondition;
        private int _initialTargetValue;
        private bool _initialIsRelative;
        
        public SelectConditionPanel(
            AutoLayerRuleCreateModal parent, 
            Action deselectAction,
            Action<bool> selectAction,
            Action<int, int, PatternCondition, bool> setPatternAction,
            int index)
        {
            _parent = parent;
            _deselectAction = deselectAction;
            _selectAction = selectAction;
            _setPatternAction = setPatternAction;
            _index = index;
            _initialCondition = _parent.Rule.Pattern[_index].Condition;
            _initialTargetValue = _parent.Rule.Pattern[_index].TargetValue;
            _initialIsRelative = _parent.Rule.Pattern[_index].IsRelative;
        }
        
        public void Execute()
        {
            var contraint = _parent.Rule.Pattern[_index];
            switch (contraint.Condition)
            {
                case PatternCondition.DontCare:
                    _setPatternAction(_index, _parent.SelectedRef?.Value ?? 0, PatternCondition.MustBe, false);
                    _selectAction(true);
                    break;
                case PatternCondition.MustBe:
                    _setPatternAction(_index, _parent.SelectedRef?.Value ?? 0, PatternCondition.MustNotBe, false);
                    _selectAction(false);
                    break;
                default:
                    _setPatternAction(_index, _parent.SelectedRef?.Value ?? 0, PatternCondition.DontCare, false);
                    _deselectAction();
                    break;
            }
        }

        public void Undo()
        {
            _setPatternAction(_index, _initialTargetValue, _initialCondition, _initialIsRelative);
            switch (_initialCondition)
            {
                case PatternCondition.DontCare:
                    _deselectAction();
                    break;
                case PatternCondition.MustBe:
                    _selectAction(true);
                    break;
                case PatternCondition.MustNotBe:
                    _selectAction(false);
                    break;
            }
        }

        public string Name { get; } = "Select Condition";
    }

    private class DeselectConditionPanel : ICommand
    {
        private readonly AutoLayerRuleCreateModal _parent;
        private readonly Action _deselectAction;
        private readonly Action<bool> _selectAction;
        private readonly Action<int, int, PatternCondition, bool> _setPatternAction;
        private int _index;
        
        private PatternCondition _initialCondition;
        private int _initialTargetValue;
        private bool _initialIsRelative;
        
        public DeselectConditionPanel(
            AutoLayerRuleCreateModal parent, 
            Action deselectAction,
            Action<bool> selectAction,
            Action<int, int, PatternCondition, bool> setPatternAction,
            int index)
        {
            _parent = parent;
            _deselectAction = deselectAction;
            _selectAction = selectAction;
            _setPatternAction = setPatternAction;
            _index = index;
            _initialCondition = _parent.Rule.Pattern[_index].Condition;
            _initialTargetValue = _parent.Rule.Pattern[_index].TargetValue;
            _initialIsRelative = _parent.Rule.Pattern[_index].IsRelative;
        }
        
        public void Execute()
        {
            var ruleData = _parent.Rule.Pattern[_index];
            _setPatternAction(_index, ruleData.TargetValue, PatternCondition.DontCare, ruleData.IsRelative);
            _deselectAction();
        }

        public void Undo()
        {
            _setPatternAction(_index, _initialTargetValue, _initialCondition, _initialIsRelative);
            switch (_initialCondition)
            {
                case PatternCondition.DontCare:
                    _deselectAction();
                    break;
                case PatternCondition.MustBe:
                    _selectAction(true);
                    break;
                case PatternCondition.MustNotBe:
                    _selectAction(false);
                    break;
            }
        }

        public string Name { get; } = "Deselect Condition Panel";
    }
    
    public void PopulateConditionsPanel()
    {
        int index = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {

                if (i == 1 && j == 1)
                {
                    var text = new TextBlock()
                    {
                        Text = "Center (Self)",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeight.Bold
                    };
                    var centerBorder = new Border()
                    {
                        Child = text,
                        Background = new SolidColorBrush(FromRef.Color.ToAvalonia()),
                        Margin = new Thickness(2),
                        Cursor = new Cursor(StandardCursorType.No)
                    };
                    ConditionsPanel?.Children.Add(centerBorder);
                    Grid.SetRow(centerBorder, (i) * 2 +1);
                    Grid.SetColumn(centerBorder, (j) * 2 +1);
                    
                    text.Foreground = new SolidColorBrush(FromRef.Color.ToAvalonia().GetAutoContrastingColor());
                    SetPattern(index, FromRef.Value, PatternCondition.MustBe, true);
                    index++;
                    continue;
                }
                
                var border = new Border()
                {
                    Child = new TextBlock()
                    {
                        Text = index.ToString()
                    },
                    Background = HatchBrush.CreateHatchBrush(Colors.Gray, angle: -45),
                    Margin = new Thickness(2),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    UseLayoutRounding = true,
                    Cursor = new Cursor(StandardCursorType.No),
                    Tag = index // Id for tracking condition index
                };


                void SelectBorder(bool leftClick = false)
                {
                    if (SelectedRef != null)
                    {
                        border.Background = new SolidColorBrush(SelectedRef.Color.ToAvalonia());
                        var icon = new Icon()
                        {
                            Value = leftClick ? "mdi-check" : "mdi-close",
                            FontSize = 60,
                            Foreground = Brushes.White,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        border.Child = icon;
                        icon.Foreground = new SolidColorBrush(SelectedRef.Color.ToAvalonia().GetAutoContrastingColor());
                    }
                    else
                    {
                        border.Background = HatchBrush.CreateHatchBrush(Colors.Gray, angle: -45);
                        border.Child = null;
                    }
                }
                void DeselectBorder()
                {
                    border.Background = HatchBrush.CreateHatchBrush(Colors.Gray, angle: -45);
                    border.Child = null;
                }
                
                border.PointerPressed += (_, e) =>
                {
                    if (!e.Properties.IsLeftButtonPressed && !e.Properties.IsRightButtonPressed) return;


                    if (e.Properties.IsRightButtonPressed && border.Tag is int contraintId_)
                    {
                        var deselectCMD = new DeselectConditionPanel(
                            this,
                            DeselectBorder,
                            SelectBorder,
                            SetPattern,
                            contraintId_
                        );
                        CommandManager.ExecuteCommand(deselectCMD);
                        return;
                    }

                    if (e.Properties.IsLeftButtonPressed && border.Tag is int contraintId)
                    {
                        var selectCMD = new SelectConditionPanel(
                            this,
                            DeselectBorder,
                            SelectBorder,
                            SetPattern,
                            contraintId
                        );
                        CommandManager.ExecuteCommand(selectCMD);
                    }
                };
                OnSelectRefChanged += (selectedRef) =>
                {
                    if (selectedRef != null)
                    {
                        border.Cursor = new Cursor(StandardCursorType.Hand);
                    }
                    else
                    {
                        border.Cursor = new Cursor(StandardCursorType.No);
                    }
                };
                
                ConditionsPanel?.Children.Add(border);
                Grid.SetRow(border, (i) * 2 +1);
                Grid.SetColumn(border, (j) * 2 +1);
                index++;
            }
        }
    }
    
    // HELPERS //
    
    private void SetPattern(
        int index,
        int targetValue,
        PatternCondition condition,
        bool isRelative = false)
    {
        var constraint = Rule.Pattern[index];
        constraint.TargetValue = targetValue;
        constraint.Condition = condition;
        constraint.IsRelative = isRelative;
        Rule.Pattern[index] = constraint;
    }
}

public class IntRefListItemControl : UserControl
{

    private IAssetScope _scope;
    
    [ExposePropToPlugin("AutoLayerEditor.IntRefList.Item")]
    public IntGridValueRef IntRef { get; }

    public IntRefContext Context { get; private init; }

    public Expander? Expander;
    
    public Icon? IconDisplay;
    public TextBlock? NameText;
    public Border? ColorDisplay;
    public Image? DefaultTileImage;
    
    public StackPanel? Body;

    public Grid? ButtonsPanel;
    public NumericIntUpDown? refValueSelector;
    public Button? EditButton;
    public Button? AddRuleButton;
    public Divider? Separator;
    public StackPanel? RulesList;
    
    public IntRefListItemControl(IntGridValueRef intRef, IntRefContext context)
    {
        _scope = EngineServices.AssetsManager.CreateAssetScope();
        Context = context;
        IntRef = intRef;
        CreateComponents();
        RegisterEvents();
        Content = Expander;
        RefreshDisplay();
        
        AutoLayerEditorIntRefListItemContext.Config config = new()
            {
                GetIntRef = () => IntRef,
                RefreshDisplay = RefreshDisplay,
            };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefListItem, this, config);
    }
    
    private void CreateComponents()
    {
        ColorDisplay = new Border()
        {
            Width = 32,
            Height = 32,
            Background = new Avalonia.Media.SolidColorBrush(IntRef.Color.ToAvalonia()),
            Margin = new Thickness(0, 0, 10, 0),
        };
        
        DefaultTileImage = new Image()
        {
            Width = 30,
            Height = 30,
            Margin = new Thickness(2),
        };
        RenderOptions.SetBitmapInterpolationMode(DefaultTileImage, BitmapInterpolationMode.None);
        ColorDisplay.Child = DefaultTileImage;
        
        if(IntRef.DefaultTileData.TilesetId == Ulid.Empty)
        {
            DefaultTileImage.Source = EditorAssets.FallbackImage;
        }
        else
        {
            var tileset = _scope.Load<BaseTilesetDef>(IntRef.DefaultTileData.TilesetId);

            if (tileset != null)
            {
                var bitmap = EngineServices.ResourcesService.Load<Bitmap>(tileset.ImagePath);

                if (bitmap != null)
                {
                    var rect = new PixelRect(
                        (int)IntRef.DefaultTileData.TilePosition.X,
                        (int)IntRef.DefaultTileData.TilePosition.Y,
                        tileset.TileWidth,
                        tileset.TileHeight
                    );

                    DefaultTileImage.Source = new CroppedBitmap(bitmap, rect);
                }
                else
                {
                    // Fallback image si le chargement échoue
                    DefaultTileImage.Source = EditorAssets.FallbackImage;
                }
            }
        }

        IconDisplay = new Icon()
        {
            Value = "mdi-alert-box-outline",
            FontSize = 20,
            Foreground = Avalonia.Media.Brushes.Red,
            Margin = new Thickness(0, 0, 10, 0),
            IsVisible = false,
        };
        
        NameText = new TextBlock()
        {
            Text = IntRef.Name,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        
        Body = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };
        
        ButtonsPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, 5, 1.4*"),
            RowDefinitions = new RowDefinitions("*, 5, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 5, 0, 5),
        };
        Body.Children.Add(ButtonsPanel);
        
        refValueSelector = new NumericIntUpDown()
        {
            Value = IntRef.Value,
            Minimum = 0,
            Maximum = 1000000,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        ButtonsPanel.Children.Add(refValueSelector);
        Grid.SetRow(refValueSelector, 2);
        Grid.SetColumn(refValueSelector, 0);
        Grid.SetColumnSpan(refValueSelector, 3);
        
        EditButton = new Button()
        {
            Content = "Edit Group",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        ButtonsPanel.Children.Add(EditButton);
        Grid.SetColumn(EditButton, 0);
        AddRuleButton = new Button()
        {
            Content = "Add Rule",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        ButtonsPanel.Children.Add(AddRuleButton);
        Grid.SetColumn(AddRuleButton, 2);
        
        Separator = new Divider()
        {
            Margin = new Thickness(0, 5, 0, 5),
        };
        Body.Children.Add(Separator);
        
        RulesList = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };
        Body.Children.Add(RulesList);
        
        Expander = new Expander()
        {
            Header = new StackPanel()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Children =
                {
                    IconDisplay!,
                    ColorDisplay!,
                    NameText!,
                }
            },
            Content = Body!,
            IsExpanded = false,
        };
    }

    private void RegisterEvents()
    {
        EditButton.Click += (_, _) =>
        {
            var editModal = new IntRefListCreateModal(IntRef);

            editModal.OnCreateIntRefConfirmed += () =>
            {
                editModal.BakeIntRef();
                editModal.Close();
            };
            editModal.OnCreateIntRefCancelled += () => { editModal.Close(); };
            editModal.ShowDialog((Window?)this.GetVisualRoot()!);
        };
        
        refValueSelector.ValueChanged += (_, _) =>
        {
            if (refValueSelector.Value != null)
            {
                IntRef.Value = (int)refValueSelector.Value;
            }
        };
        
        AddRuleButton.Click += (_, _) =>
        {
            var ruleModal = new AutoLayerRuleCreateModal(IntRef,Context);
            
            ruleModal.OnCreateRuleConfirmed += (rule) =>
            {
                Logger.Debug("[IntRefListItemControl] Created Rule for IntRef {IntRefName} : {@Rule}", IntRef.Name, rule);
                
                if(!Context.RulesByIntRefValue.TryGetValue(IntRef.Value, out List<AutoLayerRule>? value))
                {
                    value = new List<AutoLayerRule>();
                    Context.RulesByIntRefValue[IntRef.Value] = value;
                }

                value.Add(rule);
                ruleModal.Close();
                RefreshDisplay();
            };
            
            ruleModal.ShowDialog((Window?)this.GetVisualRoot()!);
        };
    }
    
    [ExposeToPlugin("AutoLayerEditor.IntRefList.Item")]
    private void RefreshDisplay()
    {
        if (ColorDisplay != null)
        {
            ColorDisplay.Background = new Avalonia.Media.SolidColorBrush(IntRef.Color.ToAvalonia());
        }
        
        if (NameText != null)
        {
            NameText.Text = IntRef.Name;
        }
        
        if(RulesList == null)
            return;
        
        RulesList.Children.Clear();

        if (Context.RulesByIntRefValue.TryGetValue(IntRef.Value, out List<AutoLayerRule>? rules))
        {
            foreach (var rule in rules)
            {
                var ruleItem = new IntRefRuleItemControl(rule, Context);
                RulesList.Children.Add(ruleItem);
            }
        }
    }
}

public class IntRefRuleItemControl : UserControl
{
    
    public AutoLayerRule Rule { get; }
    public IntRefContext Context { get; private init; }
    
    public Border? BodyBorder;
    public Grid? Body;

    public Grid? PreviewPatternCondition;
    
    public IntRefRuleItemControl(AutoLayerRule rule, IntRefContext context)
    {
        Rule = rule;
        Context = context;
        CreateComponents();
        RefreshPreviewPattern();
        Content = BodyBorder;
    }

    private void CreateComponents()
    {
        BodyBorder = new Border()
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 5, 0, 5),
            Padding = new Thickness(5),
        };
        
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("Auto, 5, *"),
        };
        BodyBorder.Child = Body;
        
        PreviewPatternCondition = new Grid()
        {
            RowDefinitions = new RowDefinitions("5, 16, 5, 16, 5, 16, 5"),
            ColumnDefinitions = new ColumnDefinitions("5, 16, 5, 16, 5, 16, 5"),
            Height = (16 * 3) + (4 * 5),
            Width = (16 * 3) + (4 * 5),
            Background = CheckerBoardBrush.CreateCheckerBoardBrush(new Color(
                5, 200, 200, 200
            ), Colors.Transparent, 10),
        };
        Body.Children.Add(PreviewPatternCondition);
        Grid.SetRow(PreviewPatternCondition, 0);
        Grid.SetColumn(PreviewPatternCondition, 0);
    }

    public void RefreshPreviewPattern()
    {
        var index = 0;
        PreviewPatternCondition.Children.Clear();
        foreach (var constraint in Rule.Pattern)
        {
            
            
            var row = index / 3;
            var col = index % 3;
            
            if(constraint.Condition == PatternCondition.DontCare)
            {
                var noneBorder = new Border()
                {
                    Width = 16,
                    Height = 16,
                    Background = HatchBrush.CreateHatchBrush(Colors.Gray, angle: -45, thickness: .1, spacing:3),
                };
                PreviewPatternCondition.Children.Add(noneBorder);
                Grid.SetRow(noneBorder, row * 2 + 1);
                Grid.SetColumn(noneBorder, col * 2 + 1);
                index++;
                continue;
            }
            
            index++;
            
            var border = new Border()
            {
                Width = 16,
                Height = 16,
                Background = new SolidColorBrush(
                    Context.IntRefs.FirstOrDefault(r => r.Value == constraint.TargetValue)?.Color.ToAvalonia() ?? Colors.Transparent),
                Child = new Icon()
                {
                    Value = constraint.Condition == PatternCondition.MustBe ? "mdi-check" : "mdi-close",
                    FontSize = 12,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            };
            PreviewPatternCondition.Children.Add(border);
            Grid.SetRow(border, row * 2 + 1);
            Grid.SetColumn(border, col * 2 + 1);
        }
    }
}

public class IntRefListControl : UserControl
{
    public Grid? Body;
    
    public IntGridTilesetDef? selectedTileset = null;

    [ExposeEventToPlugin("AutoLayerEditor.IntRefList")]
    public event Action<IntGridValueRef>? AddedIntRef;

    [ExposeEventToPlugin("AutoLayerEditor.IntRefList")]
    public event Action<IntGridValueRef>? RemovedIntRef;

    public event Action<IntRefContext>? OnCreateTileset;
    
    [ExposePropToPlugin("AutoLayerEditor.IntRefList")]
    public IntRefContext Context  { get; private init; }
    
    private int _nextIntRefId = 0;
    
    public IntRefListMenu? MenuPanel;
    
    public ScrollViewer? ListViewer;
    public StackPanel? ListBody;
    
    public IntRefListControl()
    {
        Context = new IntRefContext();
        CreateComponents();
        RegisterEvents();
        Content = Body;
        
        AutoLayerEditorIntRefListContext.Config config = new()
        {
            AddIntRef = AddIntRef,
            RemoveIntRef = RemoveIntRef,
            GetContext = () => Context,
            AddAddedIntRef = (handler) => AddedIntRef += handler,
            RemoveAddedIntRef = (handler) => AddedIntRef -= handler,
            AddRemovedIntRef = (handler) => RemovedIntRef += handler,
            RemoveRemovedIntRef = (handler) => RemovedIntRef -= handler,
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefList, this, new AutoLayerEditorIntRefListContext(config));
    }

    private void CreateComponents()
    {
        NotificationManager = new WindowNotificationManager(this.GetVisualRoot() as Window)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
        };
        
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, 5, *, 5"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            Margin = new Thickness(5, 5, 5, 20)
        };
        
        MenuPanel = new IntRefListMenu();
        Body.Children.Add(MenuPanel);
        Grid.SetRow(MenuPanel, 0);
        
        ListViewer = new ScrollViewer()
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        };
        Body.Children.Add(ListViewer);
        Grid.SetRow(ListViewer, 2);
        
        ListBody = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
        };
        ListViewer.Content = ListBody;
    }

    public WindowNotificationManager NotificationManager { get; set; }

    private void RegisterEvents()
    {
        if(MenuPanel == null) return;

        
        MenuPanel.OnCreateIntRef += () =>
        {
            var createModal = new IntRefListCreateModal();

            createModal.OnCreateIntRefConfirmed += () =>
            {
                var intRef = createModal.BakeIntRef();
                
                if (intRef != null)
                {
                    AddIntRef(intRef);
                    Logger.Debug("[IntRefListControl] Created IntRef: {IntRefName}", intRef.Name);
                }

                createModal.Close();
            };
            createModal.OnCreateIntRefCancelled += () => { createModal.Close(); };
            createModal.ShowDialog((Window?)this.GetVisualRoot()!);
        };

        MenuPanel.OnSaveTileset += () =>
        {
            OnCreateTileset?.Invoke(Context);
            if(selectedTileset == null)
            {
                selectedTileset = EngineServices.AssetsManager.CreateAsset<IntGridTilesetDef>();
            }

            if (Context.IntRefs.FirstOrDefault()?.DefaultTileData.TilesetId != Ulid.Empty)
            {
                selectedTileset.TileHeight = (int)Context.IntRefs.FirstOrDefault()?.DefaultTileData.TileSize.X;
                selectedTileset.TileWidth = (int)Context.IntRefs.FirstOrDefault()?.DefaultTileData.TileSize.Y;
            }

            selectedTileset.Rules = Context.RulesByIntRefValue.Values.SelectMany(r => r).ToList();
            selectedTileset.IntRefs = Context.IntRefs.ToList();
            EngineServices.AssetsManager.GetLoadedPacks()[0].AddOrUpdateAsset(selectedTileset);
        };
        
        AddedIntRef += (_) => RefreshList();
        RemovedIntRef += (_) => RefreshList();
    }
    
    public void RefreshList()
    {
        if (ListBody == null) return;
        
        Dictionary<IntGridValueRef, bool> expandedStates = new();
        foreach (var child in ListBody.Children)
        {
            if (child is IntRefListItemControl intRefItemControl)
            {
                expandedStates[intRefItemControl.IntRef] = intRefItemControl.Expander?.IsExpanded ?? false;
            }
        }

        ListBody.Children.Clear();
        
        List<int> usedValues = Context.IntRefs.Select(r => r.Value).ToList();
        
        // Sort IntRefs by Value
        foreach (var intRef in Context.IntRefs.ToList().OrderBy(intRef => intRef.Value))
        {
            var intRefItem = new IntRefListItemControl(intRef, Context);
            
            if (usedValues.Count(v => v == intRef.Value) > 1)
            {
                intRefItem.IconDisplay!.IsVisible = true;
                ToolTip.SetTip(intRefItem.IconDisplay, "Warning: Duplicate Value detected!");
                
                intRefItem.refValueSelector!.Foreground = Avalonia.Media.Brushes.Red;
            }
            else if(intRefItem.IconDisplay!.IsVisible)
            {
                intRefItem.IconDisplay.IsVisible = false;
                ToolTip.SetTip(intRefItem.IconDisplay, null);
                intRefItem.refValueSelector!.ClearValue(ForegroundProperty);
            }
            
            intRefItem.Expander!.IsExpanded = expandedStates.ContainsKey(intRef) && expandedStates[intRef];
            ListBody.Children.Add(intRefItem);
        }
    }

    [ExposeToPlugin("AutoLayerEditor.IntRefList")]
    public void AddIntRef(IntGridValueRef intRef)
    {
        intRef.Value = _nextIntRefId++;
        Context.IntRefs.Add(intRef);
        AddedIntRef?.Invoke(intRef);
        
    }
    
    [ExposeToPlugin("AutoLayerEditor.IntRefList")]
    public void RemoveIntRef(IntGridValueRef intRef)
    {
        Context.IntRefs.Remove(intRef);
        RemovedIntRef?.Invoke(intRef);
    }

    public void LoadIntRefsFromTileset(IntGridTilesetDef tilesetDef)
    {
        Context.IntRefs.Clear();
        Context.RulesByIntRefValue.Clear();
        selectedTileset = tilesetDef;
        
        foreach (var intRef in tilesetDef.IntRefs)
        {
            Context.IntRefs.Add(intRef);
        }
        
        foreach (var rule in tilesetDef.Rules)
        {
            if(!Context.RulesByIntRefValue.TryGetValue(rule.TargetIntGridValue, out List<AutoLayerRule>? value))
            {
                value = new List<AutoLayerRule>();
                Context.RulesByIntRefValue[rule.TargetIntGridValue] = value;
            }

            value.Add(rule);
        }
        
        RefreshList();
    }

}