using System;
using System.Drawing;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;

namespace RPGCreator.UI.Common.TilesetsCommonComponents;

public class TilesetExplorer : UserControl
{
    
    private int _baseSelectedIndex = -1;

    public enum TilesetType
    {
        All,
        AutotileOnly,
        NonAutotileOnly
    }
    
    public event Action<ITileDef?>? TileSelected;
    public event Action<int>? TilesetChanged;
    
    private TilesetType _type;
    private Size _canvasSize = new Size(256, 256);
    
    private MoveableCanvas _canvas;
    private Image _previewImage;
    private Panel _body;
    private SelectionCursorControl _selectionCursor;
    private ComboBox _setSelector;
    private IAssetScope _scope;

    public TilesetExplorer(IAssetScope scope, Panel? parentBody = null, Size? canvasSize = null, int baseSelectedIndex = -1, TilesetType tilesetType = TilesetType.All)
    {
        _scope = scope ?? throw new ArgumentNullException(nameof(scope), "Asset scope cannot be null.");
        _type = tilesetType;
        if(parentBody == null)
            _body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
        else
            _body = parentBody;
        
        if(canvasSize != null)
            _canvasSize = canvasSize.Value;
        
        CreateComponents();
        
        _baseSelectedIndex = baseSelectedIndex;
        
        RegisterEvents();
        Content = _body;
    }

    private void CreateComponents()
    {
        
        _setSelector = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 0, 5),
            PlaceholderText = "Select Tileset..."
        };
        _body.Children.Add(_setSelector);
        
        _canvas = new MoveableCanvas
        {
            Width = _canvasSize.Width,
            Height = _canvasSize.Height,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
            LimitTo00Coordinates = true,
            LimitToContentSize = true,
            ShowGrid = true,
            ShowCheckboard = true,
            CheckboardSize = 32
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
        _selectionCursor = new SelectionCursorControl
        {
            Width = 32,
            Height = 32,
            IsVisible = false
        };
        _canvas.AddMoveableElement(_selectionCursor);
        _canvas.CanvasBody.PointerPressed += (s, e) =>
        {
            if (!e.Properties.IsLeftButtonPressed)
                return;
            e.Handled = true;
            
            var position = e.GetPosition(_canvas.CanvasBody);
            Logger.Debug("[TilesetExplorer] Canvas clicked at position: {0}", position);
            var cellSize = _canvas.GridCellSize;
            
            double alignedX = Math.Floor((position.X - (_canvas.CurrentElementsPosition.X % cellSize.Width)) / cellSize.Width) * cellSize.Width + (_canvas.CurrentElementsPosition.X % cellSize.Width);
            double alignedY = Math.Floor((position.Y - _canvas.CurrentElementsPosition.Y % cellSize.Height) / cellSize.Height) * cellSize.Height + (_canvas.CurrentElementsPosition.Y % cellSize.Height);
            
            Logger.Debug("[TilesetExplorer] Aligned position: {0}, {1}", alignedX, alignedY);
            Canvas.SetLeft(_selectionCursor, alignedX);
            Canvas.SetTop(_selectionCursor, alignedY);
            _canvas.UpdateOrigin(_selectionCursor);
            _selectionCursor.IsVisible = true;
            ITileDef? tileToPaint = null;
            if (_setSelector?.SelectedItem is SetOptionItem selectedItem)
            {
                var def = _scope.Load<BaseTilesetDef>(selectedItem.AssetId);
                
                var tilePositionInTileset = new Point( // Row and Column in tileset
                    (int)((position.X + Math.Abs(_canvas.CurrentElementsPosition.X)) / cellSize.Width),
                    (int)((position.Y + Math.Abs(_canvas.CurrentElementsPosition.Y)) / cellSize.Height)
                );

                var tilesetInstance = EngineServices.GameFactory.CreateInstance<ITilesetInstance>(def);
                
                tileToPaint = tilesetInstance.GetTileAt(tilePositionInTileset.X, tilePositionInTileset.Y);
                Logger.Debug("[TilesetExplorer] Created tile definition at position {0} in tileset {1}", tilePositionInTileset, def.Name);
                
                TileSelected?.Invoke(tileToPaint);
            }
        };
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        _canvas.SetGridCellSize(new Size(32, 32));
        ClearTilesetOptions();
        Logger.Debug("[TilingPanel] Loading tileset options...");
        var searchResults = EngineServices.AssetsManager.SearchAllPacks<BaseTilesetDef>();
        foreach (var result in searchResults)
        {
            var def = _scope.Load<BaseTilesetDef>(result.AssetId);

            bool canAutotile = def is IAutotileDef;
            
            if (_type == TilesetType.AutotileOnly && !canAutotile)
                continue;
            if (_type == TilesetType.NonAutotileOnly && canAutotile)
                continue;
            
            AddTilesetOption(def);
            Logger.Debug("[TilingPanel] Added tileset option from search: {0}", def.Name);
        }
        if(_baseSelectedIndex >= 0)
            _setSelector.SelectedIndex = _baseSelectedIndex;
    }
    
    private void RegisterEvents()
    {
        _setSelector.SelectionChanged += SetSelectorOnSelectionChanged;
    }

    public void AddTilesetOption(BaseTilesetDef definition)
    {
        _setSelector?.Items.Add(new SetOptionItem(definition));
    }
    
    public void ClearTilesetOptions()
    {
        _setSelector?.Items.Clear();
    }
    
    private void SetSelectorOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        TilesetChanged?.Invoke(_setSelector.SelectedIndex);
        
        if (_setSelector?.SelectedItem is SetOptionItem selectedItem)
        {
            Logger.Debug("[TilingPanel] Selected tileset: {0}", selectedItem.Name);
            var def = _scope.Load<BaseTilesetDef>(selectedItem.AssetId);
            if (_previewImage != null)
                _previewImage.Source = EngineServices.ResourcesService.Load<Bitmap>(def.ImagePath);
        }
        
    }
}