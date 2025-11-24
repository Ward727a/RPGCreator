using Newtonsoft.Json;

namespace RPGCreator.Core.Serializer;

public class EngineSerializer
{
    public static EngineSerializer Instance { get; } = new EngineSerializer();
    
    private readonly JsonSerializerSettings _settings;
    
    private EngineSerializer()
    {
        _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new EngineJsonConverter() },
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
    }
    
    public void Serialize<T>(T obj, out string data)
    {
        var json = JsonConvert.SerializeObject(obj, _settings);
        data = json;
    }
    
    public void Deserialize<T>(string data, out T obj)
    {
        obj = JsonConvert.DeserializeObject<T>(data, _settings)!;
    }
    
    public void Deserialize<T>(string data, out T obj, out Type type)
    {
        obj = JsonConvert.DeserializeObject<T>(data, _settings)!;
        type = obj!.GetType();
    }
}