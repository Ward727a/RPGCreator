using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK.Attributes;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;

[SerializingType("EntityLayerDef")]
public class EntityLayerDefinition() : LayerWithElements<EntitySpawner>
{
    protected override LayerChunk<EntitySpawner> CreateChunkInstance() => new EntityLayerChunk();
}