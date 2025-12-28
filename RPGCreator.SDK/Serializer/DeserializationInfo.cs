namespace RPGCreator.SDK.Serializer;
public sealed class DeserializationInfo
{
    private readonly IDataValueReader _reader;

    public DeserializationInfo(IDataValueReader reader)
    {
        _reader = reader;
    }

    public bool TryGetValue<T>(string name, out T? value)
    {
        try
        {
            value = _reader.ReadValue<T>(name);
            return value != null;
        }
        catch (Exception ex)
        {
            value = default;
            return false;
        }
    }
    
    public bool TryGetValue<T>(string name, out T value, T defaultValue)
    {
        try
        {
            value = _reader.ReadValue<T>(name);
            if (value == null)
            {
                value = defaultValue;
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            value = defaultValue;
            return false;
        }
    }

    public bool TryGetList<T>(string name, out List<T>? value)
    {
        return TryGetValue(name, out value);
    }

    public bool TryGetDictionary<TKey, TValue>(string name, out Dictionary<TKey, TValue>? value)
        where TKey : notnull
    {
        return TryGetValue(name, out value);
    }
    
    public IEnumerable<string> GetAvailableKeys()
    {
        return _reader.GetKeys();
    }
}