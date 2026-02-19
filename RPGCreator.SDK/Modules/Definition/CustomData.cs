using System.Globalization;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Modules.Definition;

/// <summary>
/// A custom data system, also called "data bags" or "property bags", that allows modules to store and retrieve misc data, defined by users or other modules.
/// </summary>
[SerializingType("CustomData")]
public class CustomData : ISerializable, IDeserializable
{
    public event Action<string>? OnDataChanged;
    public event Action<string>? OnDataRemoved;
    
    private Dictionary<string, object> _data = new();
    
    public CustomData Set<T>(string key, T value)
    {
        if (value == null) return this;
        _data[key] = value;
        OnDataChanged?.Invoke(key);
        return this;
    }

    public CustomData Get<T>(string key, out T? value)
    {
        value = this.GetAs<T>(key);
        return this;
    }
    
    public CustomData GetOrDefault<T>(string key, T defaultValue, out T value)
    {
        value = this.GetAsOrDefault(key, defaultValue);
        return this;
    }
    
    public CustomData Remove(string key, out bool removed)
    {
        removed = Remove(key);
        return this;
    }
    
    public IEnumerable<string> Keys => _data.Keys;

    public void Clear()
    {
        _data.Clear();
    }
    
    public T? GetAs<T>(string key)
    {
        if (_data.TryGetValue(key, out var value))
        {
            if (value is T typedValue) return typedValue;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return default;
            }
        }
        return default;
    }
    
    public Type? GetTypeOf(string key)
    {
        if (_data.TryGetValue(key, out var value))
        {
            return value.GetType();
        }
        return null;
    }
    
    public T GetAsOrDefault<T>(string key, T defaultValue)
    {
        if (_data.TryGetValue(key, out var value))
        {
            try
            {
                if (value is T typedValue) return typedValue;
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
        return new SerializationInfo(typeof(CustomData)).AddValue("Store", _data ?? new Dictionary<string, object>());
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        foreach (var value in _data.Values)
        {
            if (value is Ulid ulidValue)
            {
                referencedIds.Add(ulidValue);
            }
            else if (value is IEnumerable<Ulid> ulidEnumerable)
            {
                referencedIds.AddRange(ulidEnumerable);
            }
        }
        return referencedIds;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetDictionary("Store", out _data);
        _data ??= new Dictionary<string, object>();
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