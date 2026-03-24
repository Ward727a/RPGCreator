using System.Collections;
using System.Globalization;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Modules.Definition;

// DevNote: There is still some room for optimisation here. Notably, the string hashing for dictionary keys is not very efficient.

/// <summary>
/// A custom data system, also called "data bags" or "property bags", that allows modules to store and retrieve misc data, defined by users or other modules.
/// </summary>
[SerializingType("CustomData")]
public class CustomData : ISerializable, IDeserializable, IDisposable, ICloneable
{
    object ICloneable.Clone() => Clone();

    /// <summary>
    /// Event triggered when a data value is changed.<br/>
    /// The event parameter is the key of the changed data.
    /// </summary>
    public event Action<string>? DataChanged;
    
    /// <summary>
    /// Event triggered when a data value is removed.<br/>
    /// The event parameter is the key of the removed data.
    /// </summary>
    public event Action<string>? OnDataRemoved;
    
    private Dictionary<string, object> _data = new();
    
    private CustomData(Dictionary<string, object> initialData) : this()
    {
        _data = new Dictionary<string, object>(initialData.Count);
        foreach (var kvp in initialData)
        {
            var val = CloneValue(kvp.Value);
            if(val != null)
                _data[kvp.Key] = val;
            else
                Logger.Error("Failed to clone value for key {key}(Val: {value}) in CustomData.", kvp.Key, kvp.Value);
        }
    }

    private object? CloneValue(object? value)
    {
        if (value == null) return null;
        
        if (value is string || value.GetType().IsValueType) 
            return value;

        if (value is ICloneable cloneable)
        {
            return cloneable.Clone();
        }

        if (value is Array arr)
        {
            return arr.Clone();
        }

        if (value is IList list)
        {
            var listType = value.GetType();
            var newList = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in list)
            {
                newList.Add(CloneValue(item));
            }
            
            return newList;
        }
                
        return value;
    }

    public CustomData()
    {
    }

    public CustomData Set<T>(string key, T value)
    {
        if (value == null) return this;
        _data[key] = value;
        DataChanged?.Invoke(key);
        return this;
    }

    public CustomData Get<T>(string key, out T? value)
    {
        value = GetAs<T>(key);
        return this;
    }
    
    public CustomData GetOrDefault<T>(string key, T defaultValue, out T value)
    {
        value = GetAsOrDefault(key, defaultValue);
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
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
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
        var clone = new CustomData(_data);
        return clone;
    }

    public void Dispose()
    {
        DisposeEvents();
        _data.Clear();
    }

    public void DisposeEvents()
    {
        DataChanged = null;
        OnDataRemoved = null;
    }
}