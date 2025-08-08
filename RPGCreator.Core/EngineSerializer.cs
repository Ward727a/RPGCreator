
using System.Collections;
using System.Text;
using System.Xml.Linq;
using Serilog;

namespace RPGCreator.Core;

/*
 *
 * RPG Creator Engine Serializer
 * ============================
 * This class is responsible for serializing and deserializing objects
 * Author: RPG Creator Team (Ward).
 *
 * DevNote:
 * I still need to clean up the code, and make it more readable. (Done in someway, but it probably needs more work [Ward727, 15/07/2025])
 * I would want to add more features to it, like the support for custom output formats (like JSON, YAML, etc.) with easy extensibility. (LATER ON, FIRST VERSION IS XML ONLY)
 * But for now, I need to focus on cleaning all of this. [Ward727, 14/07/2025]
 * ============================
 * I added a version to the object data, but for now it's hardcoded, I still need to think about how to handle versioning in the future. [Ward727, 15/07/2025]
 * ============================
 * I want to add a way to reduce types in the string data, for this I could use a mapping dictionary that maps types to strings, and then add a <TypeMapping> element at the top of the XML data.
 * For this, I could see to automatically generate the mapping dictionary inside the SerializationInfo class (ex: AddValue => Check if the type is already in the mapping dictionary, if not, add it).
 * [Ward727, 18/07/2025]
 * 
 */

public class EngineSerializer
{
    public static EngineSerializer Instance = null!;

    internal EngineSerializer()
    {
        Log.Information($"EngineSerializer started at {DateTime.Now}.");
        Instance = this;
    }

    /// <summary>
    /// Serializes an object whose type implements <see cref="ISerializable"/>.
    /// </summary>
    /// <param name="obj">Object that need to be serialized</param>
    /// <param name="data">The serialized data of the object</param>
    /// <param name="save">[DEBUG] True if the output data should be saved in a file or not</param>
    /// <typeparam name="T">A class that implements <see cref="ISerializable"/>.</typeparam>
    public void Serialize<T>(T obj, out string data, bool save = true) where T: class, ISerializable
    {
        data = "<NULL/>";

        var info = obj.GetObjectData();
        
        if (info == null)
        {
            Log.Error("Serialization failed: Object data is null.");
            return;
        }

        data = GetData(info);
        
        Log.Information("Serialization completed successfully.");

        if (!save) return;
        
        // Write the data to a file in the application's data directory
        string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string filePath = Path.Combine(appDataDirectory, $"{obj.GetType().Name}.xml");
        try
        {
            File.WriteAllText(filePath, data);
            Log.Information($"Serialized data written to {filePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to write serialized data to file: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get the data from the serialization info and convert it to a string.
    /// </summary>
    /// <param name="info">The serialization info from a class that implements <see cref="ISerializable"/></param>
    /// <returns></returns>
    internal string GetData(SerializationInfo info)
    {
        string data = "<NULL/>";
        
        // Write the serialization info to a string
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<Object Type=\"{info.ObjectType.FullName}\" Assembly=\"{info.AssemblyName}\" QualifiedName=\"{info.QualifiedName}\" SerializerVersion=\"1.0\">");
        foreach (var entryName in info.GetNames())
        {
            if(info.TryGetValue(entryName, out var entryValue, out var entryType) == false)
            {
                Log.Error($"Failed to get value for {entryName}");
                continue;
            }
            
            if (TryGetListEntry(entryType, entryName, entryValue, sb) || TryGetDictionaryEntry(entryType, entryName, entryValue, sb))
                continue;

            if(entryValue is SerializationInfo serializationInfo)
            {
                sb.AppendLine($"  <Value V=\"{entryName}\" Type=\"{entryType.AssemblyQualifiedName}\">");
                sb.AppendLine(GetData(serializationInfo));
                sb.AppendLine("  </Value>");
                continue;
            }
            
            sb.AppendLine($"  <Value V=\"{entryName}\" Type=\"{entryType.AssemblyQualifiedName}\">");
            sb.AppendLine($"    {entryValue}");
            sb.AppendLine("  </Value>");
        }
        sb.AppendLine("</Object>");
        data = sb.ToString();
        Log.Information("Serialization completed successfully.");

        return data;
    }
    
    /// <summary>
    /// Deserializes a data string into an object and its type.
    /// </summary>
    /// <param name="data">The data string got from <see cref="Serialize{T}"/></param>
    /// <param name="obj">The object remade from the data string</param>
    /// <param name="type">The type of the object</param>
    public void Deserialize(string data, out object? obj, out System.Type? type)
    {
        obj = null;
        type = null;

        if (string.IsNullOrEmpty(data) || data == "<NULL/>")
        {
            Log.Error("Deserialization failed: Data is null or empty.");
            return;
        }

        try
        {
            var info = new DeserializationInfo(data);
            obj = info.GetObject();
            type = info.ObjectType;
            if (obj == null)
            {
                Log.Error("Deserialization failed: Object is null.");
                return;
            }
            Log.Information("Deserialization completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Error($"Deserialization failed: {ex.Message}");
        }

        
    }
    
    /// <summary>
    /// Try to get a list entry from the serialization info and append it to the StringBuilder.
    /// </summary>
    /// <param name="entryType">The type of the entry, should be a List[SerializationInfo.SerializationListEntry]</param>
    /// <param name="entryName">The name of the entry</param>
    /// <param name="entryValue">The value of the entry, should be a List of SerializationInfo.SerializationListEntry</param>
    /// <param name="sb">The StringBuilder to append the data to</param>
    /// <returns>True if the entry was a list, false otherwise</returns>
    protected bool TryGetListEntry(System.Type? entryType, string entryName, object? entryValue, StringBuilder sb)
    {
        if (entryType != typeof(List<SerializationInfo.SerializationListEntry>)) return false;
        
        sb.AppendLine($"  <List N=\"{entryName}\" Type=\"{entryType.GetGenericArguments()[0].AssemblyQualifiedName}\">");
                
        foreach (var o in (IList)entryValue)
        {
            if (o is not SerializationInfo.SerializationListEntry entryList) continue;
            
            if (entryList.Value is SerializationInfo _info)
            {
                sb.AppendLine($"    <Item Type=\"{entryList.Type}\">");
                sb.AppendLine(
                    GetData(_info));
                sb.AppendLine("    </Item>");
            }
            else
            {
                sb.AppendLine($"    <Item Type=\"{entryList.Value.GetType().AssemblyQualifiedName}\">");
                sb.AppendLine($"    {entryList.Value}");
                sb.AppendLine("    </Item>");
            }
        }
        sb.AppendLine("  </List>");
        return true;

    }
    
    /// <summary>
    /// Try to get a dictionary entry from the serialization info and append it to the StringBuilder.
    /// </summary>
    /// <param name="entryType">The type of the entry, should be a Dictionary[KeyType, SerializationInfo.SerializationListEntry]</param>
    /// <param name="entryName">The name of the entry</param>
    /// <param name="entryValue">The value of the entry, should be a Dictionary of SerializationInfo.SerializationListEntry</param>
    /// <param name="sb">The StringBuilder to append the data to</param>
    /// <returns>True if the entry was a dictionary, false otherwise</returns>
    protected bool TryGetDictionaryEntry(System.Type? entryType, string entryName, object? entryValue, StringBuilder sb)
    {
        if (entryType.GetInterface("IDictionary") == null) return false;
        
        sb.AppendLine($"  <Dictionary N=\"{entryName}\" KeyType=\"{entryType.GetGenericArguments()[0].AssemblyQualifiedName}\" ValueType=\"{typeof(SerializationInfo.SerializationListEntry).AssemblyQualifiedName}\">");
        foreach (DictionaryEntry entry in (IDictionary)entryValue)
        {
            SerializationInfo.SerializationListEntry value = (SerializationInfo.SerializationListEntry)entry.Value;
            sb.AppendLine($"    <Item Type=\"{value.Type}\" Key=\"{entry.Key}\">");
            if (entry.Value is SerializationInfo.SerializationListEntry entryList)
            {
                if (entryList.Value is SerializationInfo _info)
                {
                    sb.AppendLine(GetData(_info));
                }
                else
                {
                    sb.AppendLine($"      {entryList.Value}");
                }
            }
            sb.AppendLine("    </Item>");
        }
        sb.AppendLine("  </Dictionary>");
        return true;
    }
}

/// <summary>
/// This interface should be implemented by classes that need to be serialized.
/// </summary>
/// <remarks>
/// To use this interface, the class must implement a constructor that takes no parameters.
/// </remarks>
public interface ISerializable
{
    /// <summary>
    /// This method should return a <see cref="SerializationInfo"/> object that contains the data to be serialized.<br/>
    /// It should include all the properties and fields that need to be serialized.
    /// </summary>
    /// <returns><see cref="SerializationInfo"/> object containing the data to be serialized</returns>
    public SerializationInfo GetObjectData();
}
/// <summary>
/// This interface should be implemented by classes that need to be deserialized.
/// </summary>
/// <remarks>
/// To use this interface, the class must implement a constructor that takes no parameters.
/// </remarks>
public interface IDeserializable
{
    /// <summary>
    /// This method should set the object data from the <see cref="SerializationInfo"/> object.<br/>
    /// It should work in conjunction with <see cref="ISerializable.GetObjectData"/>.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> object containing the data to be set, which should be the same as the one returned by <see cref="ISerializable.GetObjectData"/>.</param>
    public void SetObjectData(DeserializationInfo info);
}

/// <summary>
/// This class is used to hold the deserialization information of an object.
/// </summary>
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
        
        AssemblyName = XmlData.Root.Attribute("Assembly")?.Value;
        QualifiedName = XmlData.Root.Attribute("QualifiedName")?.Value;
        
        ObjectType = System.Type.GetType(QualifiedName ?? string.Empty);
        
        if (ObjectType == null)
        {
            throw new InvalidOperationException($"Type '{QualifiedName}' could not be found.");
        }
    }

    internal object? GetObject()
    {

        Log.Debug("Deserializing object of type {ObjectType} from XML data.", ObjectType?.FullName);
        if (!IsXMLValid())
        {
            Log.Error("XML data is not valid.");
            return null;
        }
        
        if (ObjectType == null)
        {
            Log.Error("Object type is null.");
            return null;
        }

        if (!ObjectType.GetInterfaces().Contains(typeof(ISerializable))) return null;
        
        var obj = (ISerializable)Activator.CreateInstance(ObjectType)!;
        
        // Create a new SerializationInfo object and populate it with the data from the XML
        var newInfo = new SerializationInfo(ObjectType);
        foreach (var entry in XmlData.Root.Elements("Value"))
        {
            Log.Debug("Processing entry: {Entry}", entry);
            _currentElement = entry;
            
            bool IsObjectValue = entry.Elements("Object").Any();
            
            var entryName = entry.Attribute("V")?.Value;
            if (entryName == null)
            {
                Log.Error("Entry name is null.");
                continue;
            }
            var entryValue = entry.Value.Trim();
            
            // Create an object based on the type specified in the XML
            var entryTypeName = entry.Attribute("Type")?.Value;
            if (entryTypeName == null)
            {
                Log.Error("Entry type name is null.");
                continue;
            }
            var entryType = System.Type.GetType(entryTypeName);
            if (entryType == null)
            {
                Log.Error($"Type '{entryTypeName}' could not be found.");
                continue;
            }

            if(entryName == "Tags")
                Log.Error($"Deserializing entry '{entryName}' with type '{entryType.FullName}' and value '{entryValue}'");

            object? value = null;
            if (IsObjectValue)
            {
                // Deserialize the object from the XML element
                var objectElement = entry.Element("Object");
                if (objectElement == null)
                {
                    Log.Error("Object element is null.");
                    continue;
                }
                var entryInfo = new DeserializationInfo(objectElement);
                if (entryInfo.ObjectType == null)
                {
                    Log.Error("Object type is null in the object element.");
                    continue;
                }
                if (entryInfo.ObjectType != entryType)
                {
                    Log.Error($"Object type '{entryInfo.ObjectType.FullName}' does not match expected type '{entryType.FullName}'.");
                    continue;
                }
                // Get the object from the DeserializationInfo
                value = entryInfo.GetObject();
                if(value.GetType() == typeof(SerializationInfo.SerializationEntry))
                    Console.WriteLine("");
            }
            else
            {
                value = ConvertStringToType(entryType, entryValue);
            }

            if (value == null)
            {
                Log.Error($"Value for entry '{entryName}' is null.");
                continue;
            }
            Log.Debug("Adding list entry '{EntryName}'.", entryName);
            AddValue(entryName, value);
        }

        foreach (var entry in XmlData.Root.Elements("List"))
        {
            Log.Debug("Processing list entry: {Entry}", entry);
            _currentElement = entry;
            var entryName = entry.Attribute("N")?.Value;
            if (entryName == null)
            {
                Log.Error("List entry name is null.");
                continue;
            }
            var entryTypeName = entry.Attribute("Type")?.Value;
            if (entryTypeName == null)
            {
                Log.Error("List entry type name is null.");
                continue;
            }
            var entryType = System.Type.GetType(entryTypeName);
            if (entryType == null)
            {
                Log.Error($"Type '{entryTypeName}' could not be found.");
                continue;
            }
            var list = new List<object>();
            foreach (var item in entry.Elements("Item"))
            {
                var typeString = item.Attribute("Type")?.Value;
                if (typeString == null)
                {
                    Log.Error("List item type is null.");
                    continue;
                }
                var itemType = System.Type.GetType(typeString);
                if (itemType == null)
                {
                    Log.Error($"Type '{typeString}' could not be found.");
                    continue;
                }
                
                object? convertedValue = null;
                // If the item has a "Object" element, we need to deserialize it
                if (item.Element("Object") != null)
                {
                    var objectElement = item.Element("Object");
                    if (objectElement == null)
                    {
                        Log.Error("List item object element is null.");
                        continue;
                    }
                    var entryInfo = new DeserializationInfo(objectElement);
                    convertedValue = entryInfo.GetObject();
                }
                else
                {
                    var itemValue = item.Value.Trim();
                    convertedValue = ConvertStringToType(itemType, itemValue);
                }
                
                if (convertedValue == null)
                {
                    Log.Error($"Converted value for list entry '{entryName}' is null.");
                    continue;
                }
                
                list.Add(convertedValue);
            }
            if (list.Count == 0)
            {
                Log.Debug($"List entry '{entryName}' is empty.");
                continue;
            }
            Log.Debug("Adding list entry '{EntryName}'.", entryName);
            AddValue(entryName, list);
        }
        
        foreach (var entry in XmlData.Root.Elements("Dictionary"))
        {
            Log.Debug("Processing list entry: {Entry}", entry);
            _currentElement = entry;
            var entryName = entry.Attribute("N")?.Value;
            if (entryName == null)
            {
                Log.Error("Dictionary entry name is null.");
                continue;
            }
            var keyTypeName = entry.Attribute("KeyType")?.Value;
            var valueTypeName = entry.Attribute("ValueType")?.Value;
            if (keyTypeName == null || valueTypeName == null)
            {
                Log.Error("Dictionary key or value type name is null.");
                continue;
            }
            var keyType = System.Type.GetType(keyTypeName);
            var valueType = System.Type.GetType(valueTypeName);
            if (keyType == null || valueType == null)
            {
                Log.Error($"Key or value type '{keyTypeName}' or '{valueTypeName}' could not be found.");
                continue;
            }

            var dict = new Dictionary<object, object>();
            foreach (var item in entry.Elements("Item"))
            {
                var key = item.Attribute("Key")?.Value;
                var typeString = item.Attribute("Type")?.Value;
                if (typeString == null)
                {
                    Log.Error("Dictionary item type is null.");
                    continue;
                }
                var itemType = System.Type.GetType(typeString);
                if (key == null)
                {
                    Log.Error("Dictionary item key is null.");
                    continue;
                }

                object? convertedValue = null;
                // If the item has a "Object" element, we need to deserialize it
                if (item.Element("Object") != null)
                {
                    var objectElement = item.Element("Object");
                    if (objectElement == null)
                    {
                        Log.Error("Dictionary item object element is null.");
                        continue;
                    }
                    var entryInfo = new DeserializationInfo(objectElement);
                    convertedValue = entryInfo.GetObject();
                }
                else
                {
                    var itemValue = item.Value.Trim();
                    convertedValue = ConvertStringToType(itemType, itemValue);
                }
                
                // Convert the key and value to the appropriate types
                var convertedKey = ConvertStringToType(keyType, key);
                
                if (convertedKey == null || convertedValue == null)
                {
                    Log.Error($"Converted key or value for dictionary entry '{entryName}' is null.");
                    continue;
                }
                
                dict[convertedKey] = convertedValue;
            }
            
            Log.Debug("Adding list entry '{EntryName}'.", entryName);
            AddValue(entryName, dict);
        }
        
        if(obj is IDeserializable deserializable)
            deserializable.SetObjectData(this);
        else
            throw new InvalidCastException($"Object of type {ObjectType.FullName} does not implement IDeserializable.");
        return obj;
    }

    /// <summary>
    /// Helper method to convert a string value to the appropriate type.
    /// </summary>
    /// <param name="type">The type to convert to</param>
    /// <param name="valueString">The string value to convert</param>
    /// <returns>The converted value as an object</returns>
    /// <exception cref="InvalidOperationException">Thrown if the conversion fails</exception>
    private object ConvertStringToType(System.Type type, string valueString)
    {
        Log.Debug("Trying to convert string to type: {Type}, value: {Value}", type.FullName, valueString);
        
        if (type == typeof(string))
            return valueString;
        
        var parseMethod = type.GetMethod("Parse", new[] { typeof(string) });
        if (parseMethod != null && parseMethod.IsStatic)
            return parseMethod.Invoke(null, new object?[]{valueString});

        if (type.IsEnum)
            return Enum.Parse(type, valueString);

        if (typeof(IList).IsAssignableTo(type))
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
                                Log.Error($"Type '{typeString}' does not match expected type '{awaitedType.FullName}'.");
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

        if (typeof(SerializationInfo).IsAssignableTo(type))
        {
            EngineSerializer.Instance.Deserialize(valueString, out var o, out var t);
            if (o == null || t == null)
            {
                Log.Error($"Failed to deserialize object of type {type.FullName} from string.");
                return null;
            }
            
            return o;
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

    public struct DeserializationEntry(object? value, System.Type type)
    {
        public object? Value = value;
        public System.Type Type = type;
    }

    private Dictionary<string, DeserializationEntry> _values = [];
    
    private void AddValue(string name, object? value)
    {
        if (_values.ContainsKey(name))
            return;
        
        if (value == null)
        {
            Log.Error("[EngineSerializer.Deserialization] Cannot add object {name} to DeserializationInfo: object is null.", name);
            return;
        }
        
        _values[name] = new DeserializationEntry(value, value.GetType());
    }
    public bool TryGetValue(string name, out object? value, out System.Type? type)
    {
        value = null;
        type = null;
        if (_values.TryGetValue(name, out DeserializationEntry entry))
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
        if (TryGetValue(name, out var returnedObject, out var returnedType))
        {
            if(returnedType.FullName == "RPGCreator.Core.Type.Assets.Tilesets.Autotile")
                Console.WriteLine("");
            
            if(returnedObject is T castedObject)
            {
                value = castedObject;
                
                return true;
            }
            Log.Error($"Type mismatch: expected {typeof(T).FullName}, got {returnedType.FullName}");
        }
        value = default;
        return false;
    }
    
    public bool TryGetDictionary<TKey, TValue>(string name, out Dictionary<TKey, TValue>? value)
        where TKey : notnull
        where TValue : class
    {
        if (TryGetValue(name, out var o, out var type))
        {
            if (type.GetInterfaces().Contains(typeof(IDictionary)))
            {
                value = null;
            
                if (o is Dictionary<object, object> rawDict)
                {
                    value = rawDict
                        .Where(kv => kv.Key is TKey && kv.Value is TValue)
                        .ToDictionary(
                            kv => (TKey)kv.Key,
                            kv => (TValue)kv.Value
                        );
                }
                else if (o is Dictionary<TKey, TValue> alreadyTyped)
                {
                    value = new Dictionary<TKey, TValue>(alreadyTyped);
                }
                return true;
            }
        }
        value = default;
        return false;
    }
    
    public bool TryGetList<T>(string name, out List<T>? value)
    {
        if (TryGetValue(name, out var o, out var type))
        {
            if (type.GetInterfaces().Contains(typeof(IList)))
            {
                value = null;
                if (o is IEnumerable list)
                {
                    value = list.OfType<T>().ToList();
                }

            
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
    
    public bool TryGetList<T>(string name, out List<T>? value, List<T>? defaultValue)
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
        Log.Error(errorMessage);
        value = defaultValue;
        return false;
    }
    
    public bool TryGetList<T>(string name, out List<T>? value, List<T>? defaultValue, string errorMessage)
    {
        if (TryGetList(name, out value))
        {
            return true;
        }
        Log.Error(errorMessage);
        value = defaultValue;
        return false;
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

/// <summary>
/// This class is used to hold the serialization information of an object.
/// </summary>
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

    /// <summary>
    /// This constructor initializes the SerializationInfo with the type of the object to be serialized.
    /// </summary>
    /// <param name="objectType">The type of the object to be serialized.</param>
    public SerializationInfo(System.Type objectType)
    {
        ObjectType = objectType;
        AssemblyName = objectType.Assembly.FullName;
        QualifiedName = objectType.FullName;
    }
    
    public void AddValue(string name, object? value)
    {
        if (_values.ContainsKey(name))
            return;
        
        if (value == null)
        {
            Log.Error("Cannot add object {name} to SerializationInfo: object is null.", name);
            return;
        }
        
        _values[name] = new SerializationEntry(value, value.GetType());
    }

    public void AddValue(string name, ISerializable? obj)
    {
        if (_values.ContainsKey(name))
            return;

        if (obj == null)
        {
            Log.Error("Cannot add object {name} to SerializationInfo: object is null.", name);
            return;
        }
        var value = obj.GetObjectData();
        
        _values[name] = new SerializationEntry(value, value.ObjectType);
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
                dict[entry.Key.ToString()!] = new(serializableItem.GetObjectData(), serializableItem.GetType());
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

        _values[name] = new SerializationEntry(obj, obj.GetType());
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
    
    /// <summary>
    /// This method should NOT be used directly, you need to use the method inside the DeserializationInfo class.<br/>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    internal bool TryGetValue(string name, out object? value, out System.Type? type)
    {
        value = null;
        type = null;
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

    public List<string> GetNames()
    {
        return _values.Keys.ToList();
    }

    public List<SerializationEntry> GetEntries()
    {
        return _values.Values.ToList();
    }
}