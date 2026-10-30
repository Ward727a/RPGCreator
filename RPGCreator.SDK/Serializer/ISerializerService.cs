using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Serializer;

public interface ISerializerService : IService
{
    public void Serialize(object obj, Type objType, out string data);
    public void Serialize<T>(T obj, out string data);
    public Result SerializeTo<T>(T obj, string filePath);
    public object? Deserialize(string data, Type type);
    public void Deserialize<T>(string data, out T? obj);
    public void Deserialize<T>(string data, out T obj, out Type type);
    public bool Deserialize<T>(Stream stream, out T? obj);
    public bool Deserialize(Stream stream, Type type, out object? obj);
    public void Deserialize<T>(Stream stream, out T obj, out Type type);
    public bool DeserializeFrom<T>(string filePath, [NotNullWhen(true)]out T? obj);
    
    public JsonSerializerOptions JsonOptions { get; }
}