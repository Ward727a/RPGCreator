namespace RPGCreator.SDK.Serializer;

public interface ISerializerService : IService
{
    public void Serialize<T>(T obj, out string data);
    public void SerializeTo<T>(T obj, string filePath);
    public void Deserialize<T>(string data, out T? obj);
    public void Deserialize<T>(string data, out T obj, out Type type);
    public void Deserialize<T>(Stream stream, out T? obj);
    public void Deserialize<T>(Stream stream, out T obj, out Type type);
    public void DeserializeFrom<T>(string filePath, out T? obj);
}