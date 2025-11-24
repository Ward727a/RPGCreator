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
            
            // On écrit les métadonnées de type (Important pour le polymorphisme)
            writer.WritePropertyName("$type");
            writer.WriteValue($"{info.ObjectType.FullName}, {info.AssemblyName}");

            // 2. On écrit chaque valeur du SerializationInfo
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
    {// 1. On charge le JSON dans un JObject (facile à naviguer)
        var jsonObject = JObject.Load(reader);

        // 2. On détermine le type réel (Polymorphisme)
        var typeToken = jsonObject["$type"];
        var actualType = typeToken != null ? System.Type.GetType(typeToken.ToString()) : objectType;
        
        if (actualType == null) throw new Exception("Type not found");

        // 3. On crée l'instance vide
        var instance = (IDeserializable)Activator.CreateInstance(actualType)!;

        // 4. On remplit ton DeserializationInfo (qu'on va adapter pour JSON)
        // Il faut adapter ta classe DeserializationInfo pour qu'elle accepte un JObject au lieu de XElement !
        var info = new DeserializationInfo(jsonObject); 

        // 5. On appelle ta méthode existante
        instance.SetObjectData(info);

        return instance;
    }

    public override bool CanConvert(System.Type objectType)
    {
        return typeof(ISerializable).IsAssignableFrom(objectType);
    }
}