namespace RPGCreator.SDK.Assets;

public interface IAssetTypeRegistry
{
    void RegisterMapping(string key, Type type);
    Type? GetType(string key);
    string? GetKey(Type type);
    bool HasKey(string key);
}

public static class AssetTypeKeys
{
    public const string Character = "Character";
    public const string Item = "Item";
    public const string Stat = "Stat";
    public const string Skill = "Skill";
    public const string Map = "Map";
    public const string Tileset = "Tileset";
    public const string AutoTileset = "AutoTileset";
}