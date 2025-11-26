using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RPGCreator.Core.Serializer;

public class EngineJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is ISerializable serializableObj)
        {
            var info = serializableObj.GetObjectData();

            writer.WriteStartObject();
            
            writer.WritePropertyName("$type");
            writer.WriteValue($"{info.ObjectType.FullName}, {info.AssemblyName}");

            foreach (var entryName in info.GetNames())
            {
                if (info.TryGetValue(entryName, out var entryValue))
                {
                    writer.WritePropertyName(entryName);
                    serializer.Serialize(writer, entryValue);
                }
            }

            writer.WriteEndObject();
        }
    }

    public override object? ReadJson(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        var typeToken = jsonObject["$type"];
        System.Type? actualType = null;

        if (typeToken != null)
        {
            string typeName = typeToken.ToString();
        
            // Exact match
            actualType = System.Type.GetType(typeName);

            // Loose match
            if (actualType == null && typeName.Contains(","))
            {
                var parts = typeName.Split(',');
                if (parts.Length >= 2)
                {
                    var looseTypeName = $"{parts[0].Trim()}, {parts[1].Trim()}";
                    actualType = System.Type.GetType(looseTypeName);
                }
            }
        
            // Last resort: search by simple name across all loaded assemblies
            if (actualType == null)
            {
                var parts = typeName.Split(',');
                var simpleName = parts[0].Trim(); // Juste le nom de la classe avec namespace
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    actualType = asm.GetType(simpleName);
                    if (actualType != null) break;
                }
            }
        }
        
        // Fallback
        if (actualType == null) actualType = objectType;
        
        if (actualType == null) throw new Exception($"Type not found for: {typeToken}");

        var instance = (IDeserializable)Activator.CreateInstance(actualType)!;

        var info = new DeserializationInfo(jsonObject, serializer); 

        instance.SetObjectData(info);

        return instance;
    }

    public override bool CanConvert(System.Type objectType)
    {
        return typeof(ISerializable).IsAssignableFrom(objectType);
    }
}