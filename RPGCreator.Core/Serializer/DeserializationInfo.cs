using Newtonsoft.Json.Linq;
using RPGCreator.Core.Types.Assets.Tilesets;
using Serilog;

namespace RPGCreator.Core.Serializer;
public sealed class DeserializationInfo
{
    private readonly JObject _jsonData;

    public DeserializationInfo(JObject json)
    {
        _jsonData = json;
    }

    private object? ConvertJTokenToType(JToken? token, System.Type targetType)
    {
        if (token == null) return null;
        return token.ToObject(targetType);
    }

    public bool TryGetValue(string name, out object? value, out System.Type? type)
    {
        var token = _jsonData[name];
        if (token != null)
        {
            value = token; 
            type = typeof(object); 
            return true;
        }
        value = null; type = null; return false;
    }

    public bool TryGetValue<T>(string name, out T? value)
    {
        var token = _jsonData[name];
        if (token != null)
        {
            value = token.ToObject<T>();
            return true;
        }
        value = default;
        return false;
    }
    
    public bool TryGetValue<T>(string name, out T value, T defaultValue)
    {
        var token = _jsonData[name];
        if (token != null)
        {
            value = token.ToObject<T>();
            return true;
        }
        value = defaultValue;
        return false;
    }

    public bool TryGetList<T>(string name, out List<T>? value)
    {
        return TryGetValue(name, out value);
    }

    public bool TryGetDictionary<TKey, TValue>(string name, out Dictionary<TKey, TValue>? value)
        where TKey : notnull
    {
        return TryGetValue(name, out value);
    }
}