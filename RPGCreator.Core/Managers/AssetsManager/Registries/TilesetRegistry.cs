using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

namespace RPGCreator.Core.Managers.AssetsManager.Registries;

public class TilesetRegistry : RegistryBase<BaseTilesetDef>
{
    public override string ModuleName => "tilesets";
    
    public override IEnumerable<System.Type> SupportedTypes
    {
        get
        {
            yield return typeof(BaseTilesetDef);
            yield return typeof(TilesetDef);
            yield return typeof(IntGridTilesetDef);
        }
    }
}