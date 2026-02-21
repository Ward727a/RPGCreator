using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.UI.Common;
using Ursa.Controls;
using Size = System.Drawing.Size;

namespace RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;


public class SetOptionItem : UserControl
{
    public Ulid AssetId { get; set; }
    public string Name { get; set; }
    public bool IsIntGrid { get; private set; }

    #region Components
    
    private Grid? _body;
    private Image? _previewImage;
    private TextBlock? _nameText;
    
    #endregion
    
    public SetOptionItem(BaseTilesetDef definition)
    {
        AssetId = definition.Unique;
        Name = definition.Name;
        IsIntGrid = definition is IntGridTilesetDef;
        CreateComponents();
        if (_previewImage != null && !string.IsNullOrEmpty(definition.ImagePath))
            _previewImage.Source = EngineServices.ResourcesService.Load<Bitmap>(definition.ImagePath);
        Content = _body;
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanelTilesetItem, this);
    }
    
    private void CreateComponents()
    {
        _body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("32, 10, *"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        if(!IsIntGrid)
        {
            _previewImage = new Image
            {
                Width = 32,
                Height = 32,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            _body.Children.Add(_previewImage);
            Grid.SetColumn(_previewImage, 0);
        }
        
        _nameText = new TextBlock
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Inlines = new InlineCollection()
        };
        
        
        var nameRun = new Run
        {
            Text = Name
        };

        if (IsIntGrid)
        {
            var typeRun = new Run
            {
                Text = "(IntGrid Tileset)",
                Foreground = Brushes.Gray,
                FontStyle = FontStyle.Italic,
                FontSize = 12
            };
            _nameText.Inlines.Add(typeRun);
            _nameText.Inlines.Add(new LineBreak());
        }
        _nameText.Inlines.Add(nameRun);
        
        _body.Children.Add(_nameText);
        Grid.SetColumn(_nameText, 2);
    }
}

public class SelectionCursorControl : Border
{
    private Animation _pulseAnimation;
    private System.Threading.CancellationTokenSource? _animationCancellationTokenSource;

    public SelectionCursorControl()
    {
        BorderBrush = Brushes.Cyan;
        BorderThickness = new Thickness(3);
        Background = Brushes.Transparent;

        IsHitTestVisible = false;
        
        Opacity = .8;

        InitializeAnimation();
    }

    private void InitializeAnimation()
    {
        _pulseAnimation = new Animation
        {
            Duration = TimeSpan.FromSeconds(.5),
            
            IterationCount = IterationCount.Infinite,
            
            PlaybackDirection = PlaybackDirection.Alternate, 
            
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(Border.BorderBrushProperty, Brushes.White) }
                    
                },
                
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(Border.BorderBrushProperty, Brushes.Black)}
                }
            }
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _animationCancellationTokenSource = new System.Threading.CancellationTokenSource();
        _pulseAnimation.RunAsync(this, _animationCancellationTokenSource.Token);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _animationCancellationTokenSource?.Cancel();
        _animationCancellationTokenSource = null;
        Opacity = 1.0;
    }
}


public class TilingPanelControl : UserControl
{
    private IAssetScope _scope;
    
    #region Components
    
    private StackPanel? _body;
    private Divider? _topDivider;
    private ComboBox? _setSelector;
    private Expander? _tileOptionsExpander;
    private StackPanel? _tileOptionsBody;
    private Image? _previewImage;
    private MoveableCanvas? _canvas;
    private SelectionCursorControl selectionCursor;
        
    public ListBox IntGridListBox { get; private set; }
    
    #endregion
    
    public TilingPanelControl()
    {
        _scope = EngineServices.AssetsManager.CreateAssetScope("TilingPanelControl");
        CreateComponents();
        RegisterEvents();
        Content = _body;
        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanel, this);
    }
    
    private void CreateComponents()
    {
        _body = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };
        
        _topDivider = new Divider
        {
            Orientation =  Avalonia.Layout.Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            Content = "Tiling Options"
        };
        _body.Children.Add(_topDivider);
        
        _setSelector = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 5),
            PlaceholderText = "Select Tileset..."
        };
        _body.Children.Add(_setSelector);
        
        _tileOptionsExpander = new Expander
        {
            Header = "Tile Options",
            IsExpanded = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        _body.Children.Add(_tileOptionsExpander);
        _tileOptionsBody = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };
        _tileOptionsExpander.Content = _tileOptionsBody;
        
        _canvas = new MoveableCanvas
        {
            Width = 256,
            Height = 256,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            LimitTo00Coordinates = true,
            LimitToContentSize = true,
            ShowGrid = true
        };
        _body.Children.Add(_canvas);
        
        IntGridListBox = new ListBox
        {
            Width = 256,
            Height = 256,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            IsVisible = false
        };
        _body.Children.Add(IntGridListBox);
        
        _previewImage = new Image
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        Canvas.SetLeft(_previewImage, 0);
        Canvas.SetTop(_previewImage, 0);
        _canvas.AddMoveableElement(_previewImage);
        selectionCursor = new SelectionCursorControl
        {
            Width = 32,
            Height = 32,
            IsVisible = false
        };
        _canvas.AddMoveableElement(selectionCursor);
        _canvas.CanvasBody.PointerPressed += (s, e) =>
        {
            if (!e.Properties.IsLeftButtonPressed)
                return;
            e.Handled = true;
            
            var position = e.GetPosition(_canvas.CanvasBody);
            Logger.Debug("[TilingPanel] Canvas clicked at position: {0}", position);
            var cellSize = _canvas.GridCellSize;
            
            double alignedX = Math.Floor((position.X - (_canvas.CurrentElementsPosition.X % cellSize.Width)) / cellSize.Width) * cellSize.Width + (_canvas.CurrentElementsPosition.X % cellSize.Width);
            double alignedY = Math.Floor((position.Y - _canvas.CurrentElementsPosition.Y % cellSize.Height) / cellSize.Height) * cellSize.Height + (_canvas.CurrentElementsPosition.Y % cellSize.Height);
            
            Logger.Debug("[TilingPanel] Aligned position: {0}, {1}", alignedX, alignedY);
            Canvas.SetLeft(selectionCursor, alignedX);
            Canvas.SetTop(selectionCursor, alignedY);
            _canvas.UpdateOrigin(selectionCursor);
            selectionCursor.IsVisible = true;
            ITileDef? tileToPaint = null;
            if (_setSelector?.SelectedItem is SetOptionItem selectedItem)
            {
                var def = _scope.Load<BaseTilesetDef>(selectedItem.AssetId);
                
                var tilePositionInTileset = new Point( // Row and Column in tileset
                    (int)((position.X + Math.Abs(_canvas.CurrentElementsPosition.X)) / cellSize.Width),
                    (int)((position.Y + Math.Abs(_canvas.CurrentElementsPosition.Y)) / cellSize.Height)
                );

                var tileset = EngineServices.GameFactory.CreateInstance<ITilesetInstance>(def);
                tileToPaint = tileset.GetTileAt((int)tilePositionInTileset.X, (int)tilePositionInTileset.Y);
                Logger.Debug("[TilingPanel] Created tile definition at position {0} in tileset {1}", tilePositionInTileset, def.Name);
                
                GlobalStates.BrushState.CurrentObjectToPaint = tileToPaint;
            }
        };
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        _canvas.SetGridCellSize(new Size(32, 32));
        ClearTilesetOptions();
        Logger.Debug("[TilingPanel] Loading tileset options...");
        var searchResults = EngineServices.AssetsManager.GetAssets<BaseTilesetDef>();
        foreach (var result in searchResults)
        {
            AddTilesetOption(result);
            Logger.Debug("[TilingPanel] Added tileset option from search: {0}", result.Name);
        }
    }
    
    private void RegisterEvents()
    {
        _setSelector.SelectionChanged += SetSelectorOnSelectionChanged;
        IntGridListBox.SelectionChanged += (s, e) =>
        {
            if (IntGridListBox.SelectedItem is ListBoxItem selectedTextBlock)
            {
                Logger.Debug("[TilingPanel] Selected IntGrid reference: {0}", selectedTextBlock.Content as string);
                GlobalStates.BrushState.CurrentObjectToPaint = selectedTextBlock.Tag as IntGridData;
            }
        };
    }

    public void AddTilesetOption(BaseTilesetDef definition)
    {
        _setSelector?.Items.Add(new SetOptionItem(definition));
    }
    
    public void ClearTilesetOptions()
    {
        _setSelector?.Items.Clear();
    }
    
    #region EventsHandler
    private void SetSelectorOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        if (_setSelector?.SelectedItem is SetOptionItem selectedItem)
        {
            Logger.Debug("[TilingPanel] Selected tileset: {0}", selectedItem.Name);
            var def = _scope.Load<BaseTilesetDef>(selectedItem.AssetId);
            if (def is IntGridTilesetDef intgrid)
            {
                IntGridListBox.IsVisible = true;
                _canvas.IsVisible = false;
                IntGridListBox.Items.Clear();
                foreach (var intRef in intgrid.IntRefs)
                {
                    var listItem = new ListBoxItem()
                    {
                        Content = $"Value: {intRef.Value} - Name: {intRef.Name}",
                        Tag = new IntGridData()
                        {
                            IntGridRef = intRef,
                            IntGridTilesetDef = intgrid
                        }
                    };
                    IntGridListBox.Items.Add(listItem);
                }

                if (intgrid.IntRefs.Count <= 0)
                {
                    var noItem = new TextBlock
                    {
                        Text = $"No IntGrid references found in this tileset."
                    };
                    IntGridListBox.Items.Add(noItem);
                    IntGridListBox.IsEnabled = false;
                }
                    
                return;
            }
            IntGridListBox.IsVisible = false;
            _canvas.IsVisible = true;
            if (_previewImage != null)
                _previewImage.Source = EngineServices.ResourcesService.Load<Bitmap>(def.ImagePath);
        }
        
    }
    #endregion
}