using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Maps;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class MapRegistry : RegistryBase<IMapDef>
{
    public override IEnumerable<Type> SupportedTypes => new[]
    {
        typeof(IMapDef),
        typeof(MapDefinition)
    };
    public override string ModuleName => "Maps";
}