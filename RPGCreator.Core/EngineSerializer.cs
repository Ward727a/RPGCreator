
using System.Collections;
using System.Xml.Linq;

namespace RPGCreator.Core;

public class EngineSerializer
{
    public static EngineSerializer Instance = null!;

    public EngineSerializer()
    {
        Console.WriteLine("EngineSerializer started at " + DateTime.Now + ".");
        Instance = this;
    }

    public void Serialize<T>(T obj, out string data, bool save = true) where T: class, ISerializable
    {

        data = "<NULL/>";

        var info = obj.GetObjectData();
        
        if (info == null)
        {
            Console.WriteLine("Serialization failed: Object data is null.");
            return;
        }

        data = GetData(info);
        
        Console.WriteLine("Serialization completed successfully.");

        if (!save) return;
        
        // Write the data to a file in the application's data directory
        string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(appDataDirectory, $"{obj.GetType().Name}.xml");
        try
        {
            File.WriteAllText(filePath, data);
            Console.WriteLine($"Serialized data written to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write serialized data to file: {ex.Message}");
        }
    }
    
    public void Deserialize(string data, out object? obj, out System.Type? type)
    {
        obj = null;
        type = null;

        if (string.IsNullOrEmpty(data) || data == "<NULL/>")
        {
            Console.WriteLine("Deserialization failed: Data is null or empty.");
            return;
        }

        try
        {
            // Here you would parse the XML and populate the SerializationInfo object
            // For simplicity, this example assumes the data is already in the correct format
            // You would need to implement XML parsing logic here
            Console.WriteLine("Deserialization completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization failed: {ex.Message}");
            return;
        }

        obj = info; // Replace with actual deserialized object
        type = info.ObjectType; // Replace with actual type of the deserialized object
    }

    public string GetData(SerializationInfo info)
    {
        string data = "<NULL/>";
        
        // Write the serialization info to a string
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<Object Type=\"{info.ObjectType.FullName}\" Assembly=\"{info.AssemblyName}\" QualifiedName=\"{info.QualifiedName}\">");
        foreach (var entryName in info.GetNames())
        {
            if(info.TryGetValue(entryName, out var entryValue, out var entryType) == false)
            {
                Console.WriteLine($"Failed to get value for {entryName}");
                continue;
            }
            sb.AppendLine($"  <Value V=\"{entryName}\" Type=\"{entryType.AssemblyQualifiedName}\">");
            if (entryType == typeof(List<SerializationInfo.SerializationListEntry>))
            {
                foreach (var o in (IList)entryValue)
                {
                    if (o is SerializationInfo.SerializationListEntry entryList)
                    {
                        if (entryList.Value is SerializationInfo _info)
                        {
                            sb.AppendLine(
                                GetData(_info));
                        }
                        else
                        {
                            sb.AppendLine($"    {entryList.Value}");
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine($"    {entryValue}");
            }
            sb.AppendLine("  </Value>");
        }
        sb.AppendLine("</Object>");
        data = sb.ToString();
        Console.WriteLine("Serialization completed successfully.");

        return data;
    }
}

public interface ISerializable
{
    public SerializationInfo GetObjectData();
}
public interface IDeserializable
{
    public void SetObjectData(SerializationInfo info);
}

public sealed class DeserializationInfo
{
    public string Data { get; private set; }
    public XDocument XmlData { get; private set; } = new("<NULL/>");
    public System.Type? ObjectType { get; private set; }
    public string? AssemblyName { get; private set; }
    public string? QualifiedName { get; private set; }
    
    public DeserializationInfo(string data)
    {
        Data = data;
        if (string.IsNullOrEmpty(data) || data == "<NULL/>")
        {
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        }
        try
        {
            XmlData = XDocument.Parse(data);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse XML data.", ex);
        }
        // ObjectType = 
        AssemblyName = XmlData.Root.Attribute("Assembly")?.Value;
        QualifiedName = XmlData.Root.Attribute("QualifiedName")?.Value;
        
        ObjectType = System.Type.GetType(QualifiedName ?? string.Empty);
        
        if (ObjectType == null)
        {
            throw new InvalidOperationException($"Type '{QualifiedName}' could not be found.");
        }
    }

    public object? GetObject()
    {

        if (!IsXMLValid())
        {
            Console.WriteLine("XML data is not valid.");
            return null;
        }
        
        if (ObjectType == null)
        {
            Console.WriteLine("Object type is null.");
            return null;
        }

        if (!ObjectType.IsSubclassOf(typeof(ISerializable))) return null;
        
        var obj = (ISerializable)Activator.CreateInstance(ObjectType)!;
        var info = obj.GetObjectData();
        if (info == null)
        {
            Console.WriteLine("Failed to get object data.");
            return null;
        }
            
        foreach (var entry in XmlData.Root.Elements("Value"))
        {
            var entryName = entry.Attribute("V")?.Value;
            if (entryName == null)
            {
                Console.WriteLine("Entry name is null.");
                continue;
            }
                
            if (!info.TryGetValue(entryName, out var value, out var type))
            {
                Console.WriteLine($"Failed to get value for {entryName}");
                continue;
            }
            
            info.AddValue(entryName, value);
        }
            
        return obj;

    }

    private bool IsXMLValid()
    {
        return XmlData.Root != null &&
               XmlData.Root.Name == "Object" &&
               XmlData.Root.Attribute("Type") != null &&
               XmlData.Root.Attribute("Assembly") != null &&
               XmlData.Root.Attribute("QualifiedName") != null;
    }
}
public sealed class SerializationInfo
{
    public System.Type ObjectType { get; private set; }
    public string? AssemblyName { get; private set; }
    public string? QualifiedName { get; private set; }

    public struct SerializationEntry(object? value, System.Type type)
    {
        public object? Value = value;
        public System.Type Type = type;
    }
 
    public struct SerializationListEntry(object? value, System.Type type)
    {
        public object? Value = value;
        public System.Type Type = type;
    }
    
    private Dictionary<string, SerializationEntry> _values = [];

    public SerializationInfo(System.Type objectType)
    {
        ObjectType = objectType;
        AssemblyName = objectType.Assembly.FullName;
        QualifiedName = objectType.FullName;
    }
    
    public void AddValue(string name, object value)
    {
        if (_values.ContainsKey(name))
            return;
        
        _values[name] = new SerializationEntry(value, value.GetType());
    }

    public void AddValue(string name, ISerializable obj)
    {
        if (_values.ContainsKey(name))
            return;
        var value = obj.GetObjectData();
        
        _values[name] = new SerializationEntry(value, value.GetType());
    }

    public void AddValue(string name, IDictionary obj)
    {
        if (_values.ContainsKey(name))
            return;
        var dict = new Dictionary<string, SerializationListEntry?>();
        foreach (DictionaryEntry entry in obj)
        {
            if (entry.Value is ISerializable serializableItem)
            {
                dict[entry.Key.ToString()!] = new(serializableItem.GetObjectData(), typeof(SerializationEntry));
            }
            else
            {
                dict[entry.Key.ToString()!] = new (entry.Value, entry.Value?.GetType() ?? typeof(object));
            }
        }
        _values[name] = new SerializationEntry(dict, dict.GetType());
    }
    public void AddValue(string name, IList obj)
    {
        if (_values.ContainsKey(name))
            return;
        
        var list = new List<SerializationListEntry>();
        foreach (var item in obj)
        {
            if (item is ISerializable serializableItem)
            {
                list.Add(new(serializableItem.GetObjectData(), typeof(SerializationEntry)));
            }
            else
            {
                list.Add(new SerializationListEntry(item, item.GetType()));
            }
        }
        _values[name] = new SerializationEntry(list, list.GetType());
    }
    
    public void SetValue(string name, object obj)
    {
        if (!_values.ContainsKey(name))
            return;
        
        object? value = null;

        _values[name] = new SerializationEntry(value, obj.GetType());
    }
    
    public void SetValue(string name, ISerializable obj)
    {
        if (!_values.ContainsKey(name))
            return;

        var value = obj.GetObjectData();
        _values[name] = new SerializationEntry(value, obj.GetType());
    }
    
    public void SetValue(string name, IDictionary obj)
    {
        if (!_values.ContainsKey(name))
            return;

        var dict = new Dictionary<string, SerializationListEntry?>();
        
        foreach (DictionaryEntry entry in obj)
        {
            if (entry.Value is ISerializable serializableItem)
            {
                dict[entry.Key.ToString()!] = new(serializableItem.GetObjectData(), typeof(SerializationEntry));
            }
            else
            {
                dict[entry.Key.ToString()!] = new(entry.Value, entry.Value?.GetType() ?? typeof(object));
            }
        }

        _values[name] = new SerializationEntry(dict, dict.GetType());
    }
    
    public void SetValue(string name, IEnumerable obj)
    {
        
        if (!_values.ContainsKey(name))
            return;
        
        var list = new List<object>();
        foreach (var item in obj)
        {
            if (item is ISerializable serializableItem)
            {
                list.Add(serializableItem.GetObjectData());
            }
            else
            {
                list.Add(item);
            }
        }

        _values[name] = new SerializationEntry(list, list.GetType());
    }
    
    public void DeleteValue(string name)
    {
        if (_values.ContainsKey(name))
        {
            _values.Remove(name);
        }
    }
    
    public bool TryGetValue(string name, out object? value, out System.Type? type)
    {
        // Implementation for retrieving a value from the serialization info
        if (_values.TryGetValue(name, out SerializationEntry entry))
        {
            value = entry.Value;
            type = entry.Type;
            return true;
        }
        
        value = null;
        type = null;
        return false;
    }

    public bool TryGetValue<T>(string name, out T? value)
    {
        if (TryGetValue(name, out var o, out var type))
        {
            if(type == typeof(T) || type.IsSubclassOf(typeof(T)))
            {
                value = (T?)o;
                
                return true;
            }
            else
            {
                Console.WriteLine($"Type mismatch: expected {typeof(T).FullName}, got {type.FullName}");
            }
        }
        value = default;
        return false;
    }
    public bool TryGetList<T>(string name, out T? value) where T : IList
    {
        if (TryGetValue(name, out var o, out var type))
        {
            if (type is SerializationListEntry)
            {
                // Create a new list of the specified type
                value = (T?)Activator.CreateInstance(typeof(T), new object[] { });
                if (value == null)
                {
                    Console.WriteLine($"Failed to create list of type {typeof(T).FullName}");
                    return false;
                }
                // Populate the list with the deserialized entries
                foreach (var entry in (IList)o)
                {
                    if (entry is SerializationListEntry listEntry)
                    {
                        if (listEntry.Value is SerializationInfo info)
                        {
                            // First we need to create the base object from the SerializationInfo
                            var item = Activator.CreateInstance(info.ObjectType);
                            
                            // Check if the item has the IDeserializable interface
                            if (item is IDeserializable deserializableItem)
                            {
                                // If it does, we can set the object data
                                deserializableItem.SetObjectData(info);
                            }
                            else
                            {
                                Console.WriteLine($"Item of type {info.ObjectType.FullName} does not implement IDeserializable.");
                            }
                            
                            if (item != null)
                            {
                                value.Add((T)item);
                            }
                        }
                        else
                        {
                            value.Add((T)listEntry.Value!);
                        }
                    }
                }
            }
            if(type == typeof(T) || type.IsSubclassOf(typeof(T)))
            {
                value = (T?)o;
                
                
                return true;
            }
            else
            {
                Console.WriteLine($"Type mismatch: expected {typeof(T).FullName}, got {type.FullName}");
            }
        }
        value = default;
        return false;
    }

    public bool TryGetValue<T>(string name, out T? value, T? defaultValue)
    {
        if (TryGetValue(name, out value))
        {
            return true;
        }
        value = defaultValue;
        return false;
    }
    
    public bool TryGetValue<T>(string name, out T? value, T? defaultValue, string errorMessage)
    {
        if (TryGetValue(name, out value))
        {
            return true;
        }
        else
        {
            Console.WriteLine(errorMessage);
            value = defaultValue;
            return false;
        }
    }
    
    public List<string> GetNames()
    {
        // Implementation for getting all values' names
        return _values.Keys.ToList();
    }

    public List<SerializationEntry> GetEntries()
    {
        return _values.Values.ToList();
    }
}