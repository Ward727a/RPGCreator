using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.PaintTargets;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;

public class EntityLayerRenderer : BaseLayerRenderer<EntityLayerDefinition>
{
    public override void Render(EntityLayerDefinition layer, long chunkId)
    {
        var chunkElements = layer.GetElements(chunkId);
        if (chunkElements == null)
            return;
        
        if(chunkElements.IsEmpty)
            return;
        
        for (int i = 0; i < chunkElements.Length; i++)
        {
            var entitySpawner  = chunkElements[i]; 
            if(entitySpawner == null)
                continue;

            var position = layer.GetElementWorldPosition(chunkId, i);
            
            RuntimeServices.RenderService.DrawEntitySpawner(entitySpawner, position);
        }
    }
}

[SerializingType("EntityLayerDef")]
public class EntityLayerDefinition() : LayerWithElements<EntitySpawner>
{
    protected override LayerChunk<EntitySpawner> CreateChunkInstance() => new EntityLayerChunk();
    public override UrnSingleModule UrnModule => "entity_layer".ToUrnSingleModule();
    
    private EntityLayerTarget? _paintTargetCache;
    private readonly EntityLayerRenderer _renderer = new();
    public override IPaintTarget? GetPaintTarget()
    {
        if(GlobalStates.MapState.CurrentMapDef == null)
            return null;
        var mapDef = GlobalStates.MapState.CurrentMapDef;
        
        if(_paintTargetCache != null && _paintTargetCache.MapDef == mapDef)
            return _paintTargetCache;
        
        _paintTargetCache = new EntityLayerTarget(this, mapDef, (int)mapDef.GridParameter.CellWidth, (int)mapDef.GridParameter.CellHeight);
        return _paintTargetCache;
    }

    public override ILayerRenderer GetRenderer() => _renderer;

    public override bool CanPaintObject(object? objectToPaint)
    {
        return objectToPaint is IEntityDefinition;
    }
}