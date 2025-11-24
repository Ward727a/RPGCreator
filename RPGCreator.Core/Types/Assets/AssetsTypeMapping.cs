using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Assets.Items;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Types.Assets;

public static class AssetsTypeMapping
{
    public enum EAssetType
    {
        Unknown,
        Character,
        Item,
        Stat,
        Map,
        Tileset,
        AutoTileset
    }
    
    private static readonly IReadOnlyDictionary<EAssetType, System.Type> AssetsTypeMap =
        new Dictionary<EAssetType, System.Type>()
        {
            { EAssetType.Character, typeof(ICharacter) },
            { EAssetType.Item, typeof(IItemData) },
            { EAssetType.Stat, typeof(IStatDef) },
            { EAssetType.Map, typeof(IMapDef) },
            { EAssetType.Tileset, typeof(TilesetDef)},
            { EAssetType.AutoTileset, typeof(AutoTilesetDef) }
        };
    
    public static KeyValuePair<EAssetType, System.Type> GetAssetType(EAssetType assetTypeE)
    {
        if (AssetsTypeMap.TryGetValue(assetTypeE, out var assetType))
        {
            return new KeyValuePair<EAssetType, System.Type>(assetTypeE, assetType);
        }
        
        throw new KeyNotFoundException($"Asset type '{assetTypeE}' not found in the mapping.");
    }

    public static KeyValuePair<EAssetType, System.Type> GetAssetType(System.Type type)
    {
        var key = AssetsTypeMap.FirstOrDefault(t => t.Value == type || t.Value.IsAssignableFrom(type)).Key;
        
        if (key != null)
        {
            return new KeyValuePair<EAssetType, System.Type>(key, type);
        }
        throw new KeyNotFoundException($"Asset type '{type.Name}' not found in the mapping.");
    }
    
    public static bool HasAssetType(EAssetType assetType)
    {
        return AssetsTypeMap.ContainsKey(assetType);
    }
    
    public static bool HasAssetType<T>()
    {
        return AssetsTypeMap.Values.Contains(typeof(T));
    }

}