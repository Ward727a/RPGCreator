
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPGCreator.Core.Serializer;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core;

/*
 *
 * RPG Creator Engine Serializer
 * ============================
 * This class is responsible for serializing and deserializing objects
 * Author: RPG Creator Team (Ward).
 *
 * DevNote:
 * I still need to clean up the code, and make it more readable. (Done in someway, but it probably needs more work [Ward727, 15/07/2025])
 * I would want to add more features to it, like the support for custom output formats (like JSON, YAML, etc.) with easy extensibility. (LATER ON, FIRST VERSION IS XML ONLY)
 * But for now, I need to focus on cleaning all of this. [Ward727, 14/07/2025]
 * ============================
 * I added a version to the object data, but for now it's hardcoded, I still need to think about how to handle versioning in the future. [Ward727, 15/07/2025]
 * ============================
 * I want to add a way to reduce types in the string data, for this I could use a mapping dictionary that maps types to strings, and then add a <TypeMapping> element at the top of the XML data.
 * For this, I could see to automatically generate the mapping dictionary inside the SerializationInfo class (ex: AddValue => Check if the type is already in the mapping dictionary, if not, add it).
 * [Ward727, 18/07/2025]
 * 
 */

public interface ISerializerFormatting
{
    public SerializationInfo UnformattedValue { get; }
    public object? FormattedValue { get; }
    public void Format(SerializationInfo info);
    public object? Unformat(DeserializationInfo info);
}

public class EngineSerializer : ISerializerService
{
    
    private readonly ScopedLogger _logger = Logger.ForContext<EngineSerializer>();
    
    private readonly JsonSerializerSettings _settings;
    
    public EngineSerializer()
    {
        _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { 
                new EngineJsonConverter(),
                new UlidJsonConverter(),
                new ColorJsonConverter(),
                new UrnJsonConverter()
            },
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
    }
    
    public void Serialize<T>(T obj, out string data)
    {
        var jo = JObject.FromObject(obj);
        string? key = RegistryServices.AssetTypeRegistry.GetKey(obj.GetType());
        if (key != null)
        {
            jo.AddFirst(new JProperty("TypeKey", key));
        }
        var json = JsonConvert.SerializeObject(obj, _settings);
        data = json;
    }

    public void Deserialize<T>(string data, out T obj)
    {
        obj = JsonConvert.DeserializeObject<T>(data, _settings)!;
    }
    
    public void Deserialize<T>(string data, out T obj, out Type type)
    {

        var dataType = GetTypeFromJsonData(data);
        if(dataType == null && typeof(T) == typeof(object))
        {
            _logger.Warning("Could not determine the type from JSON data.");
            dataType = typeof(object);
        }
        
        // TODO: Check how to determine the actual type BEFORE deserializing with Newtonsoft.Json, as otherwise we are deserializing an object of type 'Object' and not of the actual type.
        if(JsonConvert.DeserializeObject(data, dataType, _settings) is T deserializedObj)
        {
            _logger.Info("Successfully deserialized the object of type {type}.", args: dataType.Name);
            obj = deserializedObj;
        }
        else
        {
            _logger.Error("[EngineSerializer.Deserialize] Could not deserialize object of type {DataType}.", args: dataType?.FullName);
            obj = default!;
        }
        type = obj!.GetType();
    }
    
    #region Helpers

    private static Type? GetTypeFromJsonData(string data)
    {
        if (string.IsNullOrWhiteSpace(data)) return null;

        var jsonObject = JObject.Parse(data);
        var typeToken = jsonObject["$type"];
        if (typeToken == null) return null;
    
        string typeName = typeToken.ToString();

        // New strategy: Try to resolve the type using the AssetTypeRegistry first
        var type = RegistryServices.AssetTypeRegistry.GetType(typeName);
        if (type != null && type != typeof(GenericBaseAssetStub)) return type;

        // Old strategy: Try to get the type directly
        type = Type.GetType(typeName);
        if (type != null) return type;

        // Fallback: Try to loosen the assembly qualification
        if (typeName.Contains(","))
        {
            var parts = typeName.Split(',');
            var looseTypeName = $"{parts[0].Trim()}, {parts[1].Trim()}";
            type = Type.GetType(looseTypeName);
        }

        return type;
    }
    
    #endregion
}
