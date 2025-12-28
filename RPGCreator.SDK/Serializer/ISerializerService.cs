namespace RPGCreator.SDK.Serializer;

public interface ISerializerService
{
    public void Serialize<T>(T obj, out string data);
    public void Deserialize<T>(string data, out T obj);
    public void Deserialize<T>(string data, out T obj, out Type type);
}