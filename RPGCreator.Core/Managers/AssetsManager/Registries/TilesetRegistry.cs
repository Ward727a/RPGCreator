using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
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
            yield return typeof(IntGridTilesetDef);
        }
    }
}