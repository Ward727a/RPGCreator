using System;
using System.Collections.Generic;
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
using RPGCreator.Core;
using RPGCreator.Core.Contexts;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.ModuleSDK.Attributes;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Types;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.AutoLayer;
using RPGCreator.UI.Common.CustomBrush;
using RPGCreator.UI.Common.TilesetsCommonComponents;
using RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;
using RPGCreator.UI.Content.Editor.TilesetSelectorComponents;
using Semi.Avalonia;
using Serilog;
using Ursa.Controls;
using Notification = Ursa.Controls.Notification;
using Size = RPGCreator.Core.Types.Internal.Size;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

// ReSharper disable MemberCanBePrivate.Global

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutoLayerEditor.Components;

public class IntRefContext
{
    public HashSet<IntGridValueRef> IntRefs { get; } = new();
}

public class IntRefListMenu : StackPanel
{
    public Action? OnCreateIntRef;
    
    public Button? CreateButton;
    
    public IntRefListMenu()
    {
        Orientation = Avalonia.Layout.Orientation.Horizontal;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        Width = 300;
        CreateComponents();
        RegisterEvents();
        UIExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefListMenu, this);
    }
    
    private void CreateComponents()
    {
        CreateButton = new Button()
        {
            Content = "Create Group",
            Width = 300,
        };
        Children.Add(CreateButton);
    }
    
    private void RegisterEvents()
    {
        if (CreateButton != null)
        {
            CreateButton.Click += (_, _) => OnCreateIntRef?.Invoke();
        }
    }
}

public class IntRefListCreateModal : Window
{
    [ExposeEventToPlugin("AutoLayerEditor.IntRefList.CreateModal")]
    public event Action? OnCreateIntRefConfirmed;
    [ExposeEventToPlugin("AutoLayerEditor.IntRefList.CreateModal")]
    public event Action? OnCreateIntRefCancelled;
    
    public IntGridValueRef? FromRef;
    public bool IsEdit => FromRef != null;
    
    public StackPanel? Body;
    
    public TextBox? NameInput;
    public ColorPicker? ColorInput;
    
    public StackPanel? ButtonsPanel;
    public Button? ConfirmButton;
    public Button? CancelButton;
    
    public IntRefListCreateModal(IntGridValueRef? @ref = null) 
    {
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
        
        UIExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefListCreateModal, this, config);
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
            Color = FromRef?.Color ?? Avalonia.Media.Colors.White,
            ColorModel = ColorModel.Rgba,
            Palette = new MaterialHalfColorPalette(),
            Margin = new Thickness(0, 0, 0, 10),
        };
        Body.Children.Add(ColorInput);
        
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
        intRef.Color = ColorInput?.Color ?? Avalonia.Media.Colors.White;
        
        return intRef;
    }
}

public class AutoLayerRuleSelectOutputTileModal : Window
{
    
    public event Action<UnifiedCroppedImage, ITileDef>? OnCreateOutputTileConfirmed;
    public event Action<int>? OnSelectTilesetChanged; 
    
    private ITileDef? _selectedTile = null;
    
    public Grid? Body;
    public TilesetExplorer? TilesetExplorer;
    public StackPanel? ButtonsPanel;
    public Button? ConfirmButton;
    public Button? CancelButton;

    private AssetScope _scope = EngineCore.Instance.Managers.Assets.CreateAssetScope();
    
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
        TilesetExplorer.TileSelected += (tile) =>
        {
            _selectedTile = tile;
        };
        TilesetExplorer.TilesetChanged += (index) =>
        {
            OnSelectTilesetChanged?.Invoke(index);
        };
        
        ConfirmButton.Click += (_, _) =>
        {
            if (_selectedTile != null)
            {
                UnifiedImage? tilesetImage = null;
                UnifiedCroppedImage? outputTile = null;

                var tileset = _selectedTile.TilesetDef;
                tilesetImage = new UnifiedImage(tileset.ImagePath);
                
                outputTile = new UnifiedCroppedImage(
                    tilesetImage,
                    new PixelRect(
                        _selectedTile.PositionInTileset.X,
                        _selectedTile.PositionInTileset.Y,
                        _selectedTile.SizeInTileset.Width,
                        _selectedTile.SizeInTileset.Height
                    )
                );
                OnCreateOutputTileConfirmed?.Invoke(outputTile, _selectedTile);
            }
        };
    }
}

public class AutoLayerRuleCreateModal : Window
{

    private event Action<IntGridValueRef?>? OnSelectRefChanged;
    
    public AutoLayerRule Rule { get; } = new AutoLayerRule();
    
    public int SelectedTilesetIndex { get; private set; } = -1;
    
    public readonly IntGridValueRef FromRef;
    public IntRefContext Context { get; private init; }

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
    public StackPanel? ButtonsPanel;
    public Button? CreateRuleButton;
    public Button? CancelButton;
    
    public AutoLayerRuleCreateModal(IntGridValueRef @ref, IntRefContext context)
    {
        FromRef = @ref;
        Context = context;
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
        
        ButtonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        Body.Children.Add(ButtonsPanel);
        Grid.SetRow(ButtonsPanel, thirdRow);
        Grid.SetColumn(ButtonsPanel, firstCol);
        Grid.SetColumnSpan(ButtonsPanel, 3);
        
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
    }

    private void RegisterEvents()
    {
        OutputTilesButton.Click += (_, _) =>
        {
            var tileModal = new AutoLayerRuleSelectOutputTileModal(SelectedTilesetIndex);
            tileModal.OnCreateOutputTileConfirmed += (outputTile, selectedTile) =>
            {
                if (outputTile != null)
                {
                    var tileBorder = new Border()
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
                        Source = outputTile.UI,
                        Width = 56,
                        Height = 56,
                    };
                    tileBorder.Child = tileImage;
                    RenderOptions.SetBitmapInterpolationMode(tileImage, BitmapInterpolationMode.None);
                    OutputTilesPanel?.Children.Add(tileBorder);

                    var outputTileData = TileData.FromTileDef(selectedTile);
                    
                    tileBorder.PointerPressed += (_, _) =>
                    {
                        OutputTilesPanel?.Children.Remove(tileBorder);
                        Rule.OutputTiles.RemoveAll(t => t.UniqueId == outputTileData.UniqueId);
                    };
                    ToolTip.SetTip(tileBorder, "Click to remove this tile from output tiles.");
                    
                    Rule.OutputTiles.Add(outputTileData);
                }
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
            
            // tileModal.OnCreateOutputTileConfirmed += () =>
            // {
            //     var outputTile = tileModal.BakeOutputTile();
            //     if (outputTile != null)
            //     {
            //         Rule.OutputTiles.Add(outputTile);
            //         var tileText = new TextBlock()
            //         {
            //             Text = $"Tile ID: {outputTile.TileId}",
            //             Margin = new Thickness(5),
            //         };
            //         OutputTilesPanel?.Children.Add(tileText);
            //     }
            //     tileModal.Close();
            // };
            // tileModal.OnCreateOutputTileCancelled += () => { tileModal.Close(); };
            tileModal.ShowDialog((Window?)this.GetVisualRoot()!);
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

            Close();
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
                Background = new SolidColorBrush(intRef.Color),
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
                        Background = new SolidColorBrush(FromRef.Color),
                        Margin = new Thickness(2),
                        Cursor = new Cursor(StandardCursorType.No)
                    };
                    ConditionsPanel?.Children.Add(centerBorder);
                    Grid.SetRow(centerBorder, (i) * 2 +1);
                    Grid.SetColumn(centerBorder, (j) * 2 +1);
                    
                    text.Foreground = new SolidColorBrush(FromRef.Color.GetAutoContrastingColor());
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
                        border.Background = new SolidColorBrush(SelectedRef.Color);
                        var icon = new Icon()
                        {
                            Value = leftClick ? "mdi-check" : "mdi-close",
                            FontSize = 60,
                            Foreground = Brushes.White,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        };
                        border.Child = icon;
                        icon.Foreground = new SolidColorBrush(SelectedRef.Color.GetAutoContrastingColor());
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
                        border.Tag = 0;
                        DeselectBorder();
                        SetPattern(contraintId_, SelectedRef?.Value ?? 0, PatternCondition.DontCare, false);
                        return;
                    }

                    if (e.Properties.IsLeftButtonPressed && border.Tag is int contraintId)
                    {
                        var contraint = Rule.Pattern[contraintId];
                        switch (contraint.Condition)
                        {
                            case PatternCondition.DontCare:
                                SetPattern(contraintId, SelectedRef?.Value ?? 0, PatternCondition.MustBe, false);
                                SelectBorder(true);
                                break;
                            case PatternCondition.MustBe:
                                SetPattern(contraintId, SelectedRef?.Value ?? 0, PatternCondition.MustNotBe, false);
                                SelectBorder(false);
                                break;
                            default:
                                SetPattern(contraintId, SelectedRef?.Value ?? 0, PatternCondition.DontCare, false);
                                DeselectBorder();
                                break;
                        }
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
    [ExposePropToPlugin("AutoLayerEditor.IntRefList.Item")]
    public IntGridValueRef IntRef { get; }

    public IntRefContext Context { get; private init; }

    public Expander? Expander;
    
    public Icon? IconDisplay;
    public TextBlock? NameText;
    public Border? ColorDisplay;
    
    public StackPanel? Body;

    public Grid? ButtonsPanel;
    public NumericIntUpDown? refValueSelector;
    public Button? EditButton;
    public Button? AddRuleButton;
    public Divider? Separator;
    
    public IntRefListItemControl(IntGridValueRef intRef, IntRefContext context)
    {
        Context = context;
        IntRef = intRef;
        CreateComponents();
        RegisterEvents();
        Content = Expander;
        RefreshDisplay();
        IntRef.PropertyChanged += (_, _) => RefreshDisplay();
        
        AutoLayerEditorIntRefListItemContext.Config config = new()
            {
                GetIntRef = () => IntRef,
                RefreshDisplay = RefreshDisplay,
            };
        
        UIExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefListItem, this, config);
    }
    
    private void CreateComponents()
    {
        ColorDisplay = new Border()
        {
            Width = 20,
            Height = 20,
            Background = new Avalonia.Media.SolidColorBrush(IntRef.Color),
            Margin = new Thickness(0, 0, 10, 0),
        };

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
            ruleModal.ShowDialog((Window?)this.GetVisualRoot()!);
        };
    }
    
    [ExposeToPlugin("AutoLayerEditor.IntRefList.Item")]
    private void RefreshDisplay()
    {
        if (ColorDisplay != null)
        {
            ColorDisplay.Background = new Avalonia.Media.SolidColorBrush(IntRef.Color);
        }
        
        if (NameText != null)
        {
            NameText.Text = IntRef.Name;
        }
    }
}

public class IntRefRuleItemControl : UserControl
{
    
    public AutoLayerRule Rule { get; }
    
    public IntRefRuleItemControl()
    {
        
    }
}

public class IntRefListControl : UserControl
{
    public Grid? Body;

    [ExposeEventToPlugin("AutoLayerEditor.IntRefList")]
    public event Action<IntGridValueRef>? AddedIntRef;

    [ExposeEventToPlugin("AutoLayerEditor.IntRefList")]
    public event Action<IntGridValueRef>? RemovedIntRef;
    
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
        
        UIExtensionManager.ApplyExtensions(UIRegion.AutoLayerEditorIntRefList, this, new AutoLayerEditorIntRefListContext(config));
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
            RowDefinitions = new RowDefinitions("Auto, *"),
            ColumnDefinitions = new ColumnDefinitions("300"),
            Margin = new Thickness(5)
        };
        
        MenuPanel = new IntRefListMenu();
        Body.Children.Add(MenuPanel);
        Grid.SetRow(MenuPanel, 0);
        
        ListViewer = new ScrollViewer()
        {
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Margin = new Thickness(0, 5, 0, 0)
        };
        Body.Children.Add(ListViewer);
        Grid.SetRow(ListViewer, 1);
        
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
                    Log.Debug("[IntRefListControl] Created IntRef: {IntRefName}", intRef.Name);
                }

                createModal.Close();
            };
            createModal.OnCreateIntRefCancelled += () => { createModal.Close(); };
            createModal.ShowDialog((Window?)this.GetVisualRoot()!);
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
        intRef.PropertyChanged += (_, _) =>
        {
            RefreshList();
        };
    }
    
    [ExposeToPlugin("AutoLayerEditor.IntRefList")]
    public void RemoveIntRef(IntGridValueRef intRef)
    {
        Context.IntRefs.Remove(intRef);
        RemovedIntRef?.Invoke(intRef);
    }

}