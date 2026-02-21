using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.PaintTargets;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;

[SerializingType("EntityLayerDef")]
public class EntityLayerDefinition() : LayerWithElements<EntitySpawner>
{
    protected override LayerChunk<EntitySpawner> CreateChunkInstance() => new EntityLayerChunk();
    public override UrnSingleModule UrnModule => "entity_layer".ToUrnSingleModule();
    
    private EntityLayerTarget? _paintTargetCache;
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

    public override bool CanPaintObject(object? objectToPaint)
    {
        return objectToPaint is EntitySpawner;
    }
}