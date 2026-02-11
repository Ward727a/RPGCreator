using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Serializer;

public class EngineJsonConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is ISerializable serializableObj)
        {
            var info = serializableObj.GetObjectData();

            writer.WriteStartObject();

            string? typeKey = RegistryServices.AssetTypeRegistry.GetKey(value.GetType());

            if (typeKey != null)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(typeKey);
            }
            else
            {

                writer.WritePropertyName("$type");
                writer.WriteValue($"{info.ObjectType.FullName}, {info.AssemblyName}");
            }

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
        if (objectType == typeof(EntityLayerDefinition))
        {
            Logger.Error("Wtf");
        }
        var jsonObject = JObject.Load(reader);

        Type? actualType = null;
        
        var typeKeyToken = jsonObject["$type"];
        if (typeKeyToken != null)
        {
            actualType = RegistryServices.AssetTypeRegistry.GetType(typeKeyToken.ToString());
        }

        if (actualType == null || actualType == typeof(GenericBaseAssetStub))
        {
            var typeToken = jsonObject["$type"];

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
            actualType ??= objectType;
            
            if (actualType == null) throw new Exception($"Type not found for: {typeToken}");
        }

        if (actualType == null || actualType.IsAbstract || actualType.IsInterface)
            throw new Exception(
                $"Could not resolve concrete type for JSON Object. Ensure the type is registered in the AssetTypeRegistry.");

        var instance = (IDeserializable)Activator.CreateInstance(actualType)!;

        var assetDef = instance as IBaseAssetDef;
        if (assetDef != null)
        {
            assetDef.SuspendTracking();
        }

        var info = new DeserializationInfo(new JsonDataReader(jsonObject, serializer)); 

        instance.SetObjectData(info);

        
        if (assetDef != null)
        {
            assetDef.ResumeTracking();
        }
        
        return instance;
    }

    public override bool CanConvert(System.Type objectType)
    {
        return typeof(ISerializable).IsAssignableFrom(objectType);
    }
}