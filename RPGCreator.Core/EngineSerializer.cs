using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using RPGCreator.Core.Serializer;
using RPGCreator.Core.Serializer.Binder;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using JsonTypeInfoResolver = RPGCreator.SDK.Serializer.JsonTypeInfoResolver;

namespace RPGCreator.Core;

/*
 *
 * RPG Creator Engine Serializer
 * ============================
 * This class is responsible for serializing and deserializing objects
 * Author: RPG Creator Team (Ward).
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


    private readonly JsonSerializerOptions _options;
    
    public EngineSerializer()
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new JsonTypeInfoResolver(),
            WriteIndented = true,
            IgnoreReadOnlyProperties = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters = { 
                new UlidJsonConverter(),
                new ColorJsonConverter(),
                new UrnJsonConverter(),
            },
        };
    }
    
    public void Serialize<T>(T obj, out string data)
    {
        data = JsonSerializer.Serialize(obj, _options);
    }

    public void SerializeTo<T>(T obj, string filePath)
    {
        using var sw = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new Utf8JsonWriter(sw);
        JsonSerializer.Serialize(writer, obj, _options);
    }

    public void Deserialize<T>(string data, out T? obj)
    {
        obj = JsonSerializer.Deserialize<T>(data, _options);
    }
    
    public void Deserialize<T>(string data, out T obj, out Type type)
    {
        obj = JsonSerializer.Deserialize<T>(data, _options)!;
        type = obj?.GetType() ?? typeof(T);
    }

    public void Deserialize<T>(Stream stream, out T? obj)
    {
        if(!stream.CanRead)
            throw new Exception("Stream is not readable");
        obj = JsonSerializer.Deserialize<T>(stream, _options);
        if (obj is IBaseAssetDef def)
        {
            def.ResumeTracking();
        }
    }
    
    public void Deserialize<T>(Stream stream, out T obj, out Type type)
    {
        if(!stream.CanRead)
            throw new Exception("Stream is not readable");
        obj = JsonSerializer.Deserialize<T>(stream, _options)!;
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
            using Stream stream = File.Open(filePath, FileMode.Open);
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
