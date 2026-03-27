using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Editor;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.PaintTargets;

public class EntityLayerTarget : IPaintTarget
{
    public List<Vector2> PreviewPosition { get; set; }
    public object? PreviewObject { get; set; }
    
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
    
    public bool CanAcceptObject(object objectToPaint)
    {
        return objectToPaint is IEntityDefinition;
    }

    public void PaintAt(Vector2 position, object objectToPaint)
    {
        if (objectToPaint is IEntityDefinition entityDef)
        {
            _layerDef.AddElement(new EntitySpawner(entityDef, position), position);
        }
    }

    public void EraseAt(Vector2 position)
    {
        _layerDef.TryRemoveElement(position, out var _);
    }

    public void PreviewAt(Vector2 position, object objectToPreview)
    {
        // Preview functionality can be implemented here if needed
    }

    public void PreviewAt(List<Vector2> positions, object objectToPreview)
    {
        // Preview functionality can be implemented here if needed
    }

    public void ClearPreview()
    {
        if (RuntimeServices.RenderService.CurrentPreviewTarget == this)
        {
            RuntimeServices.RenderService.CurrentPreviewTarget = null;
        }
        
        PreviewObject = null;
    }
}