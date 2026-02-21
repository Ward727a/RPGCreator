using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.UI.Content.Editor.LeftPanel.TilingPanel;

namespace RPGCreator.UI.Common.TilesetsCommonComponents;

/*
 *
 * - Need to fix the autoTile type (it doesn't show the preview image, and it crashes when clicking on the tile inside the tileset preview).
 * > The crash is logic, it's due to the fact that an autoTiles DOESN'T have a tileset, so we need to handle this case.
 * > We need to only show a itemsBox with each groupset available in the autoTiles group.
 * 
 */

public class TilesetExplorer : UserControl
{
    
    private int _baseSelectedIndex = -1;
    
    private IntGridTilesetDef? intgrid;

    public enum TilesetType
    {
        All,
        AutotileOnly,
        NonAutotileOnly
    }

    public enum TileHistoryPosition
    {
        Top,
        Bottom
    }
    
    public event Action<object?>? TileSelected;
    public event Action<int>? TilesetChanged;
    
    private TilesetType _type;
    private Size _canvasSize = new Size(256, 256);
    
    private Panel _body;
    private ComboBox _setSelector;
    private MoveableCanvas _canvas;
    private SelectionCursorControl _selectionCursor;
    private Image _previewImage;
    private ListBox _intGridGroupsListBox;
    private WrapPanel _tileHistoryPanel;
    private IAssetScope _scope;

    public TilesetExplorer(
        IAssetScope scope, 
        Panel? parentBody = null, 
        Size? canvasSize = null, 
        int baseSelectedIndex = -1, 
        TilesetType tilesetType = TilesetType.All)
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
        
        _intGridGroupsListBox = new ListBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            IsVisible = false,
            ItemTemplate = new FuncDataTemplate<IntGridData>((@ref, scope) =>
            {
                if (@ref == null) return null;
                return new ListBoxItem()
                {
                    Content = $"Value: {@ref.IntGridRef.Value} - Name: {@ref.IntGridRef.Name}",
                };
            })
        };
        _body.Children.Add(_intGridGroupsListBox);
        
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
                Logger.Debug("[TilesetExplorer] Got tile definition at position {0} in tileset {1}", tilePositionInTileset, def.Name);
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
        _ = Task.Run(() =>
        {
            var searchResults = EngineServices.AssetsManager.GetAssetsOfType<BaseTilesetDef>();
            foreach (var def in searchResults)
            {
                bool canAutotile = def is IAutotileDef;
            
                if (_type == TilesetType.AutotileOnly && !canAutotile)
                    continue;
                if (_type == TilesetType.NonAutotileOnly && canAutotile)
                    continue;
            
                Dispatcher.UIThread.Post(()=>AddTilesetOption(def));
                Logger.Debug("[TilingPanel] Added tileset option from search: {0}", def.Name);
            }
            if(_baseSelectedIndex >= 0)
                _setSelector.SelectedIndex = _baseSelectedIndex;
        });
    }
    
    public void SetTilesetType(TilesetType type)
    {
        _ = Task.Run(() =>
        {
            if (_type == type)
                return;
            _type = type;
            Dispatcher.UIThread.Post(ClearTilesetOptions);
            Logger.Debug("[TilingPanel] Loading tileset options for type {0}...", type);
            var searchResults = EngineServices.AssetsManager.GetAssetsOfType<BaseTilesetDef>();
            foreach (var result in searchResults)
            {
                bool canAutotile = result is IAutotileDef;

                if (_type == TilesetType.AutotileOnly && !canAutotile)
                    continue;
                if (_type == TilesetType.NonAutotileOnly && canAutotile)
                    continue;

                Dispatcher.UIThread.Post(()=>AddTilesetOption(result));
                Logger.Debug("[TilingPanel] Added tileset option from search: {0}", result.Name);
            }
        });
    }
    
    private void RegisterEvents()
    {
        _setSelector.SelectionChanged += SetSelectorOnSelectionChanged;
        _intGridGroupsListBox.SelectionChanged += (sender, args) =>
        {
            if (_intGridGroupsListBox.SelectedItem is IntGridData data)
            {
                Logger.Debug("[TilesetExplorer] Selected IntGrid group: {0}", data);
                TileSelected?.Invoke(data);
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
    
    private void SetSelectorOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        TilesetChanged?.Invoke(_setSelector.SelectedIndex);
        
        if (_setSelector?.SelectedItem is SetOptionItem selectedItem)
        {
            Logger.Debug("[TilingPanel] Selected tileset: {0}", selectedItem.Name);
            var def = _scope.Load<BaseTilesetDef>(selectedItem.AssetId);
            if (def is IntGridTilesetDef gridTilesetDef)
            {
                _intGridGroupsListBox.IsVisible = true;
                _canvas.IsVisible = false;
                intgrid = gridTilesetDef;
                
                var listData = new List<IntGridData>();
                
                foreach (var intGridValueRef in intgrid.IntRefs)
                {
                    var data = new IntGridData()
                    {
                        IntGridRef = intGridValueRef,
                        IntGridTilesetDef = gridTilesetDef
                    };
                    listData.Add(data);
                }

                _intGridGroupsListBox.ItemsSource = listData;
                
                if (gridTilesetDef.IntRefs.Count <= 0)
                {
                    var noItem = new TextBlock
                    {
                        Text = $"No rules group found in this tileset."
                    };
                    _intGridGroupsListBox.Items.Add(noItem);
                    _intGridGroupsListBox.IsEnabled = false;
                }
            }
            else
            {
                _intGridGroupsListBox.IsVisible = false;
                _canvas.IsVisible = true;
                if (_previewImage != null)
                    _previewImage.Source = EngineServices.ResourcesService.Load<Bitmap>(def.ImagePath);
            }
        }
        
    }
}