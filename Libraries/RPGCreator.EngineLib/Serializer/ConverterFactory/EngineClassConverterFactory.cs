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
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.Shared.Types;
using Logger = RPGCreator.SDK.Common.Logging.Logger;

namespace RPGCreator.EngineLib.Serializer.ConverterFactory;

public class EngineClassConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IEngineClass).IsAssignableFrom(typeToConvert) 
               && ClassesRegistry.HasType(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(
            typeof(GenericEngineClassConverter<>).MakeGenericType(typeToConvert)
        );
    }
}

public class GenericEngineClassConverter<T> : JsonConverter<T> where T : class, IEngineClass
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        Utf8JsonReader searchReader = reader;
        URN classUrnDefined = URN.Empty;
        Ulid uniqueDefined = Ulid.Empty;

        // Get the ClassUrn property from the JSON file without using the default reader given, as we need it for later.
        while (searchReader.Read())
        {
            if (searchReader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (searchReader.TokenType == JsonTokenType.PropertyName)
            {
                if (searchReader.ValueTextEquals("ClassUrn"))
                {
                    searchReader.Read();
                    classUrnDefined = URN.Parse(searchReader.GetString());
                }
                else if (searchReader.ValueTextEquals("Unique"))
                {
                    searchReader.Read();
                    uniqueDefined = Ulid.Parse(searchReader.GetString());
                }
                
                if(uniqueDefined != Ulid.Empty && classUrnDefined != URN.Empty)
                    break;

                searchReader.Skip();
            }
        }

        Result<object> classInstanceResult;
        Result<Type> classTypeResult;

        if (classUrnDefined == URN.Empty && uniqueDefined == Ulid.Empty)
        {
            classInstanceResult = Result<object>.Ok(EngineServices.AssetsManager.Create<T>(ClassesRegistry.GetUrn(typeof(T)).Value).Value);
            Logger.Error("JSON asset file doesn't has a \"ClassUrn\" property, as such, the asset will be created with the default class URN instance. This can cause issue like crash, errors, and more!");
            if (classInstanceResult.Value is T typedValue)
            {
                classUrnDefined = typedValue.ClassUrn;
                uniqueDefined = typedValue.Unique;
            }
            classTypeResult = Result<Type>.Ok(typeof(T));
        }
        else
        {
            classTypeResult = ClassesRegistry.GetType(classUrnDefined);
        }

        var innerOptions = new JsonSerializerOptions(options);
        var factory = innerOptions.Converters.FirstOrDefault(c => c is EngineClassConverterFactory);
        if (factory != null) innerOptions.Converters.Remove(factory);

        var deserialized = JsonSerializer.Deserialize(ref reader, classTypeResult.Value, innerOptions);

        if(deserialized is not T)
        {
            Logger.Error("Deserialized type '{deserializedType}' is not assignable to '{targetType}'", deserialized?.GetType().Name, typeof(T).Name);
            return null;
        }
        
        if (deserialized is T deserializedTyped)
        {
            deserializedTyped.ClassUrn = classUrnDefined;
            deserializedTyped.Unique = uniqueDefined;
            return deserializedTyped;
        }
        
        Logger.Error("Failed to deserialize class '{classUrn}'", classUrnDefined);

        return null;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var type = value.GetType();
        var innerOptions = new JsonSerializerOptions(options);
        var factory = innerOptions.Converters.FirstOrDefault(c => c is EngineClassConverterFactory);
        if (factory != null) innerOptions.Converters.Remove(factory);
        
        writer.WriteStartObject();
        
        var urn = ClassesRegistry.GetUrn(type);
        if (urn.IsSuccess)
        {
            writer.WriteString("ClassUrn", urn.Value.ToString());
        }

        if (value.Unique != Ulid.Empty)
        {
            writer.WriteString("Unique", value.Unique.ToString());
        }
        
        using (JsonDocument doc = JsonSerializer.SerializeToDocument(value, type, innerOptions))
        {
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.NameEquals("ClassUrn") || prop.NameEquals("Unique")) continue;
            
                prop.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
}