using RPGCreator.Core.Types.Editor.Interfaces;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Types.Editor.Visual.PaintTargets;

public class EntityLayerTarget : IPaintTarget
{
    private readonly EntitiesLayerDefinition _layerDef;
    private readonly MapDefinition? _mapDef;
    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }
    
    public EntityLayerTarget(EntitiesLayerDefinition layerDef, MapDefinition map, int gridWidth, int gridHeight)
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
        return position is { X: >= 0, Y: >= 0 } && (position.X < _mapDef.Size.Width && position.Y < _mapDef.Size.Height);
    }

    public void PaintAt(Point position, object objectToPaint)
    {
        if(objectToPaint is EditorEntityVisual entityVisual)
            _layerDef.AddElement(entityVisual, position);
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