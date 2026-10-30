
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using RPGCreator.EngineLib.Serializer.Converters;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib;

[EngineService(typeof(ISerializerService))]
public class EngineSerializer : ISerializerService
{
    
    private readonly ScopedLogger _logger = Logger.ForContext<EngineSerializer>();

    public JsonSerializerOptions JsonOptions { get; }

    public EngineSerializer(ITypesRegistry typesRegistry)
    {
        JsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { 
                new UlidJsonConverter(),
                new ColorJsonConverter(),
                new UrnJsonConverter(),
                new FilePathJsonConverter(),
                new PointJsonConverter(),
                new Vector2JsonConverter(),
                new Vector3JsonConverter(),
                new Vector4JsonConverter(),
                new StringNameConverter(),
                new TileIndexJsonConverter(),
                new UInt128JsonConverter(),
                new SerializableDataConverter(this, typesRegistry),
                new ActorFeatureDataJsonConverter(typesRegistry)
            },
        };
    }

    public void Serialize(object obj, Type objType, out string data)
    {
        var json = JsonSerializer.Serialize(obj, objType, JsonOptions);
        data = json;
    }

    public void Serialize<T>(T obj, out string data)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        data = json;
    }

    public Result SerializeTo<T>(T obj, string filePath)
    {
        using var fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
        try
        {
            JsonSerializer.Serialize(fileStream, obj, JsonOptions);
            _logger.Debug("Serialized object to file: {filePath}", args: filePath);
            return Result.Ok();
        }
        catch (ArgumentNullException eNull)
        {
            return Result.Fail(eNull.Message);
        }
        catch (NotSupportedException notSupportedException)
        {
            return Result.Fail(notSupportedException.Message);
        }
    }

    public object? Deserialize(string data, Type type)
    {
        return JsonSerializer.Deserialize(data, type, JsonOptions);
    }
    
    public void Deserialize<T>(string data, out T? obj)
    {
        obj = JsonSerializer.Deserialize<T>(data, JsonOptions);
    }
    
    public void Deserialize<T>(string data, out T obj, out Type type)
    {
        obj = JsonSerializer.Deserialize<T>(data, JsonOptions)!;
        type = obj?.GetType() ?? typeof(T);
    }

    public bool Deserialize<T>(Stream stream, out T? obj)
    {
        if (!stream.CanRead)
        {
            obj = default;
            Logger.Error("Stream is not readable.");
            return false;
        }
        obj = JsonSerializer.Deserialize<T>(stream, JsonOptions);
        return obj != null;
    }

    public bool Deserialize(Stream stream, Type type, out object? obj)
    {
        if (!stream.CanRead)
        {
            obj = null;
            Logger.Error("Stream is not readable.");
            return false;
        }
        obj = JsonSerializer.Deserialize(stream, type, JsonOptions);
        return obj != null;
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
        obj = JsonSerializer.Deserialize<T>(stream, JsonOptions)!;
        type = obj?.GetType() ?? typeof(T);
    }

    public bool DeserializeFrom<T>(string filePath, out T? obj)
    {
        if (!File.Exists(filePath))
        {
            _logger.Error("File not found: {filePath}", args: filePath);
            obj = default;
            return false;
        }

        try
        {
            using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            Deserialize(stream, out obj);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error encountered while deserializing from file: {filePath}", args: filePath);
            obj = default;
            return false;
        }

    }
}
