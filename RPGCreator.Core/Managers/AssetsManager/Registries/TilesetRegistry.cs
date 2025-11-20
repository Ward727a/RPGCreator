using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class TilesetRegistry : RegistryBase<ITilesetDef>
{
    public override string ModuleName => "tilesets";
    
    public override IEnumerable<System.Type> SupportedTypes
    {
        get
        {
            yield return typeof(ITilesetDef);
            yield return typeof(TilesetDef);
            yield return typeof(AutoTilesetDef);
        }
    }
}