using RPGCreator.SDK.Attributes;
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

        Logger.Error("Unknown asset type: {key}, returning GenericAssetStub.", key);
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
    public void ScanCurrentAssembly(bool overrideExisting = false)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            var attrs = type.GetCustomAttributes(typeof(SerializingTypeAttribute), false);
            if (attrs.Length > 0)
            {
                var attr = (SerializingTypeAttribute)attrs[0];
                if(HasKey(attr.TypeId) && !overrideExisting) continue;
                RegisterMapping(attr.TypeId, type);
            }
        }
    }
    public void ScanAllEngineAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName.StartsWith("RPGCreator"));

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var attrs = type.GetCustomAttributes(typeof(SerializingTypeAttribute), false);
                if (attrs.Length > 0)
                {
                    var attr = (SerializingTypeAttribute)attrs[0];
                    if (HasKey(attr.TypeId))
                    {
                        Logger.Warning("ENGINE Asset type key '{key}' is already registered, overriding it with type '{typeName}' (old: {oldTypeName}).", attr.TypeId, type.FullName ?? "UNKNOWN", _keyToType[attr.TypeId].FullName ?? "UNKNOWN");
                    }
                    RegisterMapping(attr.TypeId, type);
                }
            }
        }
    }
}