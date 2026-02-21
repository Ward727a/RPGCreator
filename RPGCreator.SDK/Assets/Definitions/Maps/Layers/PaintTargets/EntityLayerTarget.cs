using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Editor;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.PaintTargets;

public class EntityLayerTarget : IPaintTarget
{
    private readonly EntityLayerDefinition _layerDef;
    public IMapDef? MapDef { get; }

    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }
    
    public EntityLayerTarget(EntityLayerDefinition layerDef, IMapDef map, int gridWidth, int gridHeight)
    {
        _layerDef = layerDef;
        MapDef = map;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }
    
    public bool IsValidPosition(Vector2 position)
    {
        if (MapDef == null)
            return false;
        return position is { X: >= 0, Y: >= 0 } && (position.X < MapDef.Size.Width && position.Y < MapDef.Size.Height);
    }

    public bool CanAcceptObject(object objectToPaint)
    {
        return objectToPaint is EntitySpawner;
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