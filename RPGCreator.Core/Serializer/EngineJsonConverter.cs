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
        var actualType = typeToken != null ? System.Type.GetType(typeToken.ToString()) : objectType;
        
        if (actualType == null) throw new Exception("Type not found");

        var instance = (IDeserializable)Activator.CreateInstance(actualType)!;

        var info = new DeserializationInfo(jsonObject); 

        instance.SetObjectData(info);

        return instance;
    }

    public override bool CanConvert(System.Type objectType)
    {
        return typeof(ISerializable).IsAssignableFrom(objectType);
    }
}