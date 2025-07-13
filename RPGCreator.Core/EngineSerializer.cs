
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
            
            var info = new DeserializationInfo(data);
            obj = info.GetObject();
            if (obj == null)
            {
                Console.WriteLine("Deserialization failed: Object is null.");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization failed: {ex.Message}");
        }

        
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
            // TODO: Change how the array are serialized, so that we can have a better way to handle them
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
                            sb.AppendLine($"    <Item Type=\"{entryList.Value.GetType().AssemblyQualifiedName}\">");
                            sb.AppendLine($"    {entryList.Value}");
                            sb.AppendLine("    </Item>");
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
    public XDocument XmlData { get; private set; }
    public System.Type? ObjectType { get; private set; }
    public string? AssemblyName { get; private set; }
    public string? QualifiedName { get; private set; }
    
    private XElement? _currentElement;

    public DeserializationInfo(XElement xmlElement)
    {
        if(xmlElement == null)
            throw new ArgumentException("XML element cannot be null.", nameof(xmlElement));
        Data = xmlElement.ToString();
        if (string.IsNullOrEmpty(Data) || Data == "<NULL/>")
        {
            throw new ArgumentException("Data cannot be null or empty.", nameof(Data));
        }
        XmlData = new XDocument(xmlElement);
        AssemblyName = xmlElement.Attribute("Assembly")?.Value;
        QualifiedName = xmlElement.Attribute("QualifiedName")?.Value;
        ObjectType = System.Type.GetType(QualifiedName ?? string.Empty);
        if (ObjectType == null)
        {
            throw new InvalidOperationException($"Type '{QualifiedName}' could not be found.");
        }
    }
    
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

        if (!ObjectType.GetInterfaces().Contains(typeof(ISerializable))) return null;
        
        var obj = (ISerializable)Activator.CreateInstance(ObjectType)!;
        
        // Create a new SerializationInfo object and populate it with the data from the XML
        var newInfo = new SerializationInfo(ObjectType);
        foreach (var entry in XmlData.Root.Elements("Value"))
        {
            _currentElement = entry;
            var entryName = entry.Attribute("V")?.Value;
            if (entryName == null)
            {
                Console.WriteLine("Entry name is null.");
                continue;
            }
            var entryValue = entry.Value.Trim();
            
            // Create an object based on the type specified in the XML
            var entryTypeName = entry.Attribute("Type")?.Value;
            if (entryTypeName == null)
            {
                Console.WriteLine("Entry type name is null.");
                continue;
            }
            var entryType = System.Type.GetType(entryTypeName);
            if (entryType == null)
            {
                Console.WriteLine($"Type '{entryTypeName}' could not be found.");
                continue;
            }

            if(entryName == "Tags")
                Console.WriteLine($"Deserializing entry '{entryName}' with type '{entryType.FullName}' and value '{entryValue}'");
            
            object? value = ConvertStringToType(entryType, entryValue);
            if (value == null)
            {
                Console.WriteLine($"Value for entry '{entryName}' is null.");
                continue;
            }
            newInfo.AddValue(entryName, value);
        }
        if(obj is IDeserializable deserializable)
            deserializable.SetObjectData(newInfo);
        else
            throw new InvalidCastException($"Object of type {ObjectType.FullName} does not implement IDeserializable.");
        return obj;
    }

    private object ConvertStringToType(System.Type type, string valueString)
    {
        if (type == typeof(string))
            return valueString;
        
        var parseMethod = type.GetMethod("Parse", new[] { typeof(string) });
        if (parseMethod != null && parseMethod.IsStatic)
            return parseMethod.Invoke(null, new object?[]{valueString});

        if (type.IsEnum)
            return Enum.Parse(type, valueString);

        if (type.GetInterfaces().Contains(typeof(IList)))
        {

            object? list = null;
            if (type.GetGenericArguments()[0] == typeof(SerializationInfo.SerializationListEntry))
            {
                // If the type is a list of SerializationListEntry, we need to handle it specially
                // We need to check if the next child of the current xml element is a <Object> element
                if (_currentElement?.Elements("Object").Any() == true)
                {
                    System.Type awaitedType = null;
                    // We then need to iterate through each <Object> element and create a new SerializationListEntry
                    foreach (var objElement in _currentElement.Elements("Object"))
                    {
                        var entry = new DeserializationInfo(objElement);
                        var entryValue = entry.GetObject();
                        
                        if (entryValue == null || entry.ObjectType == null)
                        {
                            return new List<object>();
                        }

                        if(awaitedType == null)
                        {
                            awaitedType = entry.ObjectType;
                            
                            list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(awaitedType))!;
                        }

                        if(list != null)
                        {
                            if (list.GetType().GetInterfaces().Contains(typeof(IList)))
                            {
                                // If the list is of type IList, we can add the entry value directly
                                ((IList)list).Add(entryValue);
                            }
                        }
                    }
                } else if (_currentElement?.Elements("Item").Any() == true)
                {
                    System.Type? awaitedType = null;
                    foreach (var itemElement in _currentElement?.Elements("Item"))
                    {
                        string typeString = itemElement.Attribute("Type")?.Value ?? "System.Object";
                        System.Type itemType = System.Type.GetType(typeString) ?? typeof(object);

                        if (awaitedType != null)
                        {
                            if (awaitedType != itemType)
                            {
                                Console.WriteLine($"Type '{typeString}' does not match expected type '{awaitedType.FullName}'.");
                                continue;
                            }
                        }
                        
                        if (list == null)
                        {
                            list = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                            awaitedType = itemType;
                        }
                        if (list is IList itemList)
                        {
                            // If the list is of type IList, we can add the item value directly
                            var itemValue = ConvertStringToType(itemType, itemElement.Value.Trim());
                            if (itemValue != null)
                            {
                                itemList.Add(itemValue);
                            }
                        }
                    }
                }

                return list;

            }
        }
        
        if (type.GetInterfaces().Contains(typeof(IDictionary)))
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(type.GetGenericArguments());
            var dict = (IDictionary)Activator.CreateInstance(dictType)!;
            foreach (var item in valueString.Split(','))
            {
                var keyValue = item.Split(':');
                if (keyValue.Length != 2)
                    continue; // Invalid key-value pair
                var key = ConvertStringToType(type.GetGenericArguments()[0], keyValue[0].Trim());
                var value = ConvertStringToType(type.GetGenericArguments()[1], keyValue[1].Trim());
                dict.Add(key, value);
            }
            return dict;
        }
        
        try
        {
            return Convert.ChangeType(valueString, type);
        }
        catch (InvalidCastException)
        {
            throw new InvalidOperationException($"Cannot convert '{valueString}' to type '{type.FullName}'.");
        }
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
            if (type.GetInterfaces().Contains(typeof(IList)))
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
                    if (entry is not SerializationListEntry listEntry) continue;
                    
                    if (listEntry.Value is SerializationInfo info)
                    {
                        var item = Activator.CreateInstance(info.ObjectType);
                            
                        if (item is IDeserializable deserializableItem)
                        {
                            // If it does, we can set the object data
                            deserializableItem.SetObjectData(info);
                        }
                        else
                        {
                            Console.WriteLine($"Item of type {info.ObjectType.FullName} does not implement IDeserializable.");
                            continue;
                        }
                        
                        value.Add((T)item);
                    }
                    else
                    {
                        value.Add((T)listEntry.Value!);
                    }
                }
                value = (T?)o;
            
            
                return true;
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
    
    public bool TryGetList<T>(string name, out T? value, T? defaultValue) where T : IList
    {
        if (TryGetList(name, out value))
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
    
    public bool TryGetList<T>(string name, out T? value, T? defaultValue, string errorMessage) where T : IList
    {
        if (TryGetList(name, out value))
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