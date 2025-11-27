using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Utils;
using Microsoft.Xna.Framework;
using RPGCreator.Core;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.ModuleSDK.UIModule;
using RPGCreator.Core.Runtimes.Context;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Editor.Context;
using RPGCreator.UI.Common;
using Serilog;
using Ursa.Controls;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;


public class SetOptionItem : UserControl
{
    public Ulid AssetId { get; set; }
    public string Name { get; set; }

    #region Components
    
    private Grid? _body;
    private Image? _previewImage;
    private TextBlock? _nameText;
    
    #endregion
    
    public SetOptionItem(ITilesetDef definition)
    {
        AssetId = definition.Unique;
        Name = definition.Name;
        CreateComponents();
        if (_previewImage != null) _previewImage.Source = definition.GetBitmap();
        Content = _body;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanelTilesetItem, this);
    }
    
    private void CreateComponents()
    {
        _body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("32, 10, *"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        _previewImage = new Image
        {
            Width = 32,
            Height = 32,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        _body.Children.Add(_previewImage);
        Grid.SetColumn(_previewImage, 0);
        
        _nameText = new TextBlock
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Text = Name
        };
        _body.Children.Add(_nameText);
        Grid.SetColumn(_nameText, 2);
    }
}

public class SelectionCursorControl : Border
    {
        private Animation _pulseAnimation;
        private System.Threading.CancellationTokenSource? _animationCts;

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
            _animationCts = new System.Threading.CancellationTokenSource();
            _pulseAnimation.RunAsync(this, _animationCts.Token);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _animationCts?.Cancel();
            _animationCts = null;
            Opacity = 1.0;
        }
    }

public class TilingPanelControl : UserControl
{
    
    private MapEditorContext _context;

    private AssetScope _scope;
    
    #region Components
    
    private StackPanel? _body;
    private Divider? _topDivider;
    private ComboBox? _setSelector;
    private Expander? _tileOptionsExpander;
    private StackPanel? _tileOptionsBody;
    private Image? _previewImage;
    private MoveableCanvas? _canvas;
    private SelectionCursorControl selectionCursor;
    
    #endregion
    
    public TilingPanelControl(MapEditorContext ctx)
    {
        _scope = EngineCore.Instance.Managers.Assets.CreateAssetScope("TilingPanelControl");
        _context = ctx;
        CreateComponents();
        RegisterEvents();
        Content = _body;
        UIExtensionManager.ApplyExtensions(UIRegion.EditorLeftPanelTilingPanel, this, _context);
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
            Log.Debug("[TilingPanel] Canvas clicked at position: {0}", position);
            var cellSize = _canvas.GridCellSize;
            
            double alignedX = Math.Floor((position.X - (_canvas.CurrentElementsPosition.X % cellSize.Width)) / cellSize.Width) * cellSize.Width + (_canvas.CurrentElementsPosition.X % cellSize.Width);
            double alignedY = Math.Floor((position.Y - _canvas.CurrentElementsPosition.Y % cellSize.Height) / cellSize.Height) * cellSize.Height + (_canvas.CurrentElementsPosition.Y % cellSize.Height);
            
            Log.Debug("[TilingPanel] Aligned position: {0}, {1}", alignedX, alignedY);
            Canvas.SetLeft(selectionCursor, alignedX);
            Canvas.SetTop(selectionCursor, alignedY);
            _canvas.UpdateOrigin(selectionCursor);
            selectionCursor.IsVisible = true;
            ITileDef? tileToPaint = null;
            if (_setSelector?.SelectedItem is SetOptionItem selectedItem)
            {
                var def = _scope.Load<ITilesetDef>(selectedItem.AssetId);
                
                // TODO : Handle all of this better, maybe in a factory method in TileDefinition? Or inside ITileDef?
                
                var tilePositionInTileset = new Point( // Row and Column in tileset
                    (int)(alignedX / cellSize.Width),
                    (int)(alignedY / cellSize.Height)
                );
                var tilSizeInTileset = new Point(
                    (int)(def.TileWidth),
                    (int)(def.TileHeight)
                );
                var UV = new Rect(
                    tilePositionInTileset.X * def.TileWidth,
                    tilePositionInTileset.Y * def.TileHeight,
                    tilSizeInTileset.Width,
                    tilSizeInTileset.Height
                );
                
                tileToPaint = new TileDefinition(new Vector2(0,0), tilSizeInTileset, tilePositionInTileset, def);
                Log.Debug("[TilingPanel] Created tile definition at position {0} in tileset {1}", tilePositionInTileset, def.Name);
                
                _context.SelectedObjectToPaint = tileToPaint;
            }
        };
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        _canvas.SetGridCellSize(new Size(32, 32));
        ClearTilesetOptions();
        Log.Debug("[TilingPanel] Loading tileset options...");
        var searchResults = EngineCore.Instance.Managers.Assets.SearchAllPacks<ITilesetDef>();
        foreach (var result in searchResults)
        {
            var def = _scope.Load<ITilesetDef>(result.AssetId);
            AddTilesetOption(def);
            Log.Debug("[TilingPanel] Added tileset option from search: {0}", def.Name);
        }
    }

    private void RegisterEvents()
    {
        _setSelector.SelectionChanged += SetSelectorOnSelectionChanged;
    }


    public void AddTilesetOption(ITilesetDef definition)
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
            Log.Debug("[TilingPanel] Selected tileset: {0}", selectedItem.Name);
            var def = _scope.Load<ITilesetDef>(selectedItem.AssetId);
            if (_previewImage != null)
                _previewImage.Source = def.GetBitmap();
        }
        
    }
    #endregion
}