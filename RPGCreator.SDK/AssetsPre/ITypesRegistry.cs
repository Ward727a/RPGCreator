using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.AssetsPre;

public interface ITypesRegistry : IService
{
    void RegisterMapping(URN key, Type type);
    Result<Type> GetType(URN key);
    Result<URN> GetKey(Type type);
    bool HasKey(URN key);
    bool HasType(Type type);
}