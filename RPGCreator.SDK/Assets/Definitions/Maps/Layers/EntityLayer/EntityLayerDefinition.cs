using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;

[SerializingType("EntityLayerDef")]
public class EntityLayerDefinition() : LayerWithElements<EntitySpawner>
{
    protected override LayerChunk<EntitySpawner> CreateChunkInstance() => new EntityLayerChunk();
    public override UrnSingleModule UrnModule => "entity_layer".ToUrnSingleModule();
}