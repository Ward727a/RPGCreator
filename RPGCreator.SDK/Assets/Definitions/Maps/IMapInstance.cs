using RPGCreator.SDK.ECS.Entities;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public interface IMapInstance
{
    Ulid Identifier { get; }
    MapDefinition Definition { get; }
    IReadOnlyList<IMapLayerInstance> TileLayers { get; }
    IMapLayerInstance PreviewLayer { get; }
}