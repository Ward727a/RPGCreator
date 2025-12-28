using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Serializer;

internal class JsonDataReader : IDataValueReader
{
    private readonly JObject _json;
    private readonly JsonSerializer _serializer;
    
    public JsonDataReader(JObject json, JsonSerializer serializer)
    {
        _json = json;
        _serializer = serializer;
    }

    public IEnumerable<string> GetKeys()
    {
        return _json.Properties().Select(p => p.Name);
    }

    public T? ReadValue<T>(string name)
    {
        if (_json.TryGetValue(name, out var token))
        {
            return token.ToObject<T>(_serializer);
        }
        return default;
    }

    public object? ReadValue(string name, Type targetType)
    {
        if (_json.TryGetValue(name, out var token))
        {
            return token.ToObject(targetType, _serializer);
        }
        return null;
    }
}