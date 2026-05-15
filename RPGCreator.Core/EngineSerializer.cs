
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using RPGCreator.Core.Serializer;
using RPGCreator.Core.Serializer.ConverterFactory;
using RPGCreator.Core.Serializer.Converters;
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
    
    private readonly JsonSerializerOptions _settings;
    
    public EngineSerializer()
    {
        _settings = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { 
                new UlidJsonConverter(),
                new ColorJsonConverter(),
                // old one, should be removed before first release, but for now we need it to avoid breaking testing data.
                // (I just don't want to restart the testing data... [Ward727, 22/02/2026])
                new UrnJsonConverter(),
                new EngineClassConverterFactory(),
                new FilePathJsonConverter()
            },
        };
    }

    public void Serialize(object obj, Type objType, out string data)
    {
        var json = JsonSerializer.Serialize(obj, objType, _settings);
        data = json;
    }

    public void Serialize<T>(T obj, out string data)
    {
        var json = JsonSerializer.Serialize(obj, _settings);
        data = json;
    }

    public void SerializeTo<T>(T obj, string filePath)
    {
        using var fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        JsonSerializer.Serialize(fileStream, obj, _settings);
    }

    public object? Deserialize(string data, Type type)
    {
        return JsonSerializer.Deserialize(data, type, _settings);
    }
    
    public void Deserialize<T>(string data, out T? obj)
    {
        obj = JsonSerializer.Deserialize<T>(data, _settings);
    }
    
    public void Deserialize<T>(string data, out T obj, out Type type)
    {
        obj = JsonSerializer.Deserialize<T>(data, _settings)!;
        type = obj?.GetType() ?? typeof(T);
    }

    public void Deserialize<T>(Stream stream, out T? obj)
    {
        if (!stream.CanRead)
        {
            obj = default;
            Logger.Error("Stream is not readable.");
            return;
        }
        obj = JsonSerializer.Deserialize<T>(stream, _settings);
    }
    
    public void Deserialize<T>(Stream stream, out T obj, out Type type)
    {
        if (!stream.CanRead)
        {
            obj = default;
            type = typeof(T);
            Logger.Error("Stream is not readable.");
            return;
        }
        obj = JsonSerializer.Deserialize<T>(stream, _settings)!;
        type = obj?.GetType() ?? typeof(T);
    }

    public void DeserializeFrom<T>(string filePath, out T? obj)
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("File not found: {filePath}", args: filePath);
            obj = default;
            return;
        }

        try
        {
            using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            Deserialize(stream, out obj);
            return;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error encountered while deserializing from file: {filePath}", args: filePath);
            obj = default;
            return;
        }

    }
}
