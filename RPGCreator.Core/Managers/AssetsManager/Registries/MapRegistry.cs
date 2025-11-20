using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class MapRegistry : RegistryBase<IMapDef>
{
    public override string ModuleName => "Maps";
}