using System.Globalization;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Modules.Definition;

/// <summary>
/// A custom data system, also called "data bags" or "property bags", that allows modules to store and retrieve misc data, defined by users or other modules.
/// </summary>
public class CustomData : ISerializable, IDeserializable
{
    public event Action<string>? OnDataChanged;
    public event Action<string>? OnDataRemoved;
    
    private Dictionary<string, string> _data = new();
    
    public void Set<T>(string key, T value)
    {
        _data[key] = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        OnDataChanged?.Invoke(key);
    }
    
    public IEnumerable<string> Keys => _data.Keys;
    
    public T? Get<T>(string key)
    {
        if (_data.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            } catch
            {
                return default;
            }
        }
        return default;
    }
    
    public T GetOrDefault<T>(string key, T defaultValue)
    {
        if (_data.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            } catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }
    
    public bool Has(string key) => _data.ContainsKey(key);

    public bool Remove(string key)
    {
        if (!_data.Remove(key)) return false;
        
        OnDataRemoved?.Invoke(key);
        return true;
    }
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CustomData)).AddValue("Store", _data ?? new Dictionary<string, string>());
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetDictionary("Store", out _data);
        if (_data == null) _data = new Dictionary<string, string>();
    }
    
    public CustomData Clone()
    {
        var clone = new CustomData();
        foreach (var kvp in _data)
        {
            clone._data[kvp.Key] = kvp.Value;
        }
        return clone;
    }
}