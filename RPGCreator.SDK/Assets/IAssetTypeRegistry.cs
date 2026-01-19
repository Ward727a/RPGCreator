namespace RPGCreator.SDK.Assets;

public interface IAssetTypeRegistry : IService
{
    void RegisterMapping(string key, Type type);
    Type? GetType(string key);
    string? GetKey(Type type);
    bool HasKey(string key);
    /// <summary>
    /// Scans the current assembly for asset type mappings and registers them.<br/>
    /// This need to be used when we have a class/struct with the <see cref="RPGCreator.SDK.Attributes.SerializingTypeAttribute"/>.
    /// </summary>
    /// <param name="overrideExisting">If true, will override existing mappings with the same key.</param>
    void ScanCurrentAssembly(bool overrideExisting = false);
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