using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Editor;

namespace RPGCreator.Core.Types.Editor.Visual.PaintTargets;

public class EntityLayerTarget : IPaintTarget
{
    private readonly EntityLayerDefinition _layerDef;
    private readonly IMapDef? _mapDef;
    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }
    
    public EntityLayerTarget(EntityLayerDefinition layerDef, IMapDef map, int gridWidth, int gridHeight)
    {
        _layerDef = layerDef;
        _mapDef = map;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }
    
    public bool IsValidPosition(Vector2 position)
    {
        if (_mapDef == null)
            return false;
        return position is { X: >= 0, Y: >= 0 } && (position.X < _mapDef.Size.Width && position.Y < _mapDef.Size.Height);
    }

    public void PaintAt(Vector2 position, object objectToPaint)
    {
        if(objectToPaint is EntitySpawner entityVisual)
            _layerDef.AddElement(entityVisual, position);
    }

    public void EraseAt(Vector2 position)
    {
        _layerDef.TryRemoveElement(position, out var _);
    }

    public void PreviewAt(Vector2 position, object objectToPreview)
    {
        // Preview functionality can be implemented here if needed
    }
}