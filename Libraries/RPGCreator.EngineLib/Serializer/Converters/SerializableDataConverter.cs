// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.Text.Json;
using System.Text.Json.Serialization;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Serializer.Converters;

public class SerializableDataConverter : JsonConverter<SerializableData>
{
    private readonly ISerializerService _serializer;
    private readonly ITypesRegistry _typesRegistry;

    public SerializableDataConverter(ISerializerService serializer, ITypesRegistry typesRegistry)
    {
        _serializer = serializer;
        _typesRegistry = typesRegistry;
    }
    
    public override SerializableData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var urnString = root.GetProperty("UrnType").GetString()!;
        var urn = new URN(urnString);
        
        ESerializableDataType dataType = ESerializableDataType.None;
        if(_typesRegistry.HasKey(urn))
            dataType = ESerializableDataType.Type;
        
        if(ClassesRegistry.HasUrn(urn))
            dataType = ESerializableDataType.Class;

        var objElement = root.GetProperty("ObjData");

        var returnType = GetType(dataType, urn);

        if (returnType == null)
            return null;
        
        var objData = _serializer.Deserialize(objElement.GetRawText(), returnType);

        return new SerializableData(urn, objData);
    }

    public override void Write(Utf8JsonWriter writer, SerializableData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("UrnType", value.UrnType.ToString());

        writer.WritePropertyName("ObjData");
        
        if (value.GetDirectData() == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.GetDirectData(), value.GetDirectData()!.GetType(), options);
        }
        
        writer.WriteEndObject();
    }

    private Type? GetType(ESerializableDataType dataType, URN typeUrn)
    {
        // if (dataType == ESerializableDataType.Type)
        // {
        //     var result = RegistryServices.Types.GetType(typeUrn);
        //     if (result.IsSuccess)
        //     {
        //         return result.Value;
        //     }
        //
        //     Logger.Error("Failed to get type for urn {0}, error: {1}", typeUrn, result.Error);
        //     return null;
        // }
        //
        // if (dataType == ESerializableDataType.Class)
        // {
        //     var result = ClassesRegistry.GetType(typeUrn);
        //     if (result.IsSuccess)
        //     {                        
        //         return result.Value;
        //     }
        //     Logger.Error("Failed to get class for urn {0}, error: {1}", typeUrn, result.Error);
        //     return null;
        // }
        
        return null;
    }
}