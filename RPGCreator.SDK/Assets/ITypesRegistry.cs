using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets;

public interface ITypesRegistry : IService
{
    void RegisterMapping(URN key, Type type);
    Result<Type> GetType(URN key);
    Result<URN> GetKey(Type type);
    bool HasKey(URN key);
    bool HasType(Type type);

    /// <summary>
    /// Scans the given assembly for asset type mappings and registers them.<br/>
    /// This need to be used when we have a class/struct with the <see cref="EngineTypeAttribute"/>.
    /// </summary>
    /// <param name="asm">The assembly to scan for asset type mappings.</param>
    /// <param name="overrideExisting">If true, will override existing mappings with the same key.</param>
    public void ScanAssembly(System.Reflection.Assembly asm, bool overrideExisting = false);
    public void UnScanAssembly(System.Reflection.Assembly asm);
}