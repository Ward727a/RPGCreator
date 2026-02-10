namespace RPGCreator.SDK.Assets;

public interface IAssetTypeRegistry : IService
{
    void RegisterMapping(string key, Type type);
    Type? GetType(string key);
    string? GetKey(Type type);
    bool HasKey(string key);

    /// <summary>
    /// Scans the given assembly for asset type mappings and registers them.<br/>
    /// This need to be used when we have a class/struct with the <see cref="RPGCreator.SDK.Attributes.SerializingTypeAttribute"/>.
    /// </summary>
    /// <param name="asm">The assembly to scan for asset type mappings.</param>
    /// <param name="overrideExisting">If true, will override existing mappings with the same key.</param>
    public void ScanAssembly(System.Reflection.Assembly asm, bool overrideExisting = false);
}