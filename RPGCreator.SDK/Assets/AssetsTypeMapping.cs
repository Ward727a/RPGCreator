using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.Assets;

public class AssetsTypeMapping : IAssetTypeRegistry
{
    private readonly Dictionary<string, Type> _keyToType = new();
    private readonly Dictionary<Type, string> _typeToKey = new();

    public void RegisterMapping(string key, Type type)
    {
        _keyToType[key] = type;
        _typeToKey[type] = key;
    }

    public Type? GetType(string key)
    {
        if (_keyToType.TryGetValue(key, out var type)) return type;

        Logger.Error($"Unknown asset type: {key}, returning GenericAssetStub.");
        return typeof(GenericAssetStub);
    }

    public string? GetKey(Type type) 
    {
        if (_typeToKey.TryGetValue(type, out var key)) return key;
    
        if(type == typeof(GenericAssetStub)) return "GenericStub";

        var foundKey = _typeToKey.FirstOrDefault(x => x.Key.IsAssignableFrom(type)).Value;
    
        if (foundKey != null) {
            _typeToKey[type] = foundKey; 
        }

        return foundKey;
    }

    public bool HasKey(string key) => _keyToType.ContainsKey(key);
}