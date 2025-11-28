using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Editor.Interfaces;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Types.Editor.Visual.PaintTargets;

public class TileLayerTarget : IPaintTarget
{
    private readonly TileLayerDefinition _layerDef;
    private readonly MapDefinition? _mapDef;
    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }
    
    public TileLayerTarget(TileLayerDefinition layerDef, MapDefinition map, int gridWidth, int gridHeight)
    {
        _layerDef = layerDef;
        _mapDef = map;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }
    
    public bool IsValidPosition(Point position)
    {
        if (_mapDef == null)
            return false;
        return position is { X: >= 0, Y: >= 0 } && (position.X < _mapDef.Size.Width * GridWidth && position.Y < _mapDef.Size.Height * GridHeight);
    }

    public void PaintAt(Point position, object objectToPaint)
    {
        if(objectToPaint is ITileDef tileDef)
            _layerDef.AddElement(tileDef, position);
    }

    public void EraseAt(Point position)
    {
        _layerDef.TryRemoveElement(position, out var _);
    }

    public void PreviewAt(Point position, object objectToPreview)
    {
        // Preview functionality can be implemented here if needed
    }
}