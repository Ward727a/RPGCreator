namespace RPGCreator.SDK.Serializer;

public interface IDataValueReader
{
    IEnumerable<string> GetKeys();
    T? ReadValue<T>(string name);
    object? ReadValue(string name, Type targetType);
}