using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Maps;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class MapRegistry : RegistryBase<IMapDef>
{
    public override string ModuleName => "Maps";
}