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
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.Core.Serializer.ConverterFactory;

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
        var classInstanceResult = EngineServices.AssetsManager.Create<T>(ClassesRegistry.GetUrn(typeof(T)).Value);
        var engineBase = (IEngineClass)classInstanceResult.Value;

        var innerOptions = new JsonSerializerOptions(options);
        var factory = innerOptions.Converters.FirstOrDefault(c => c is EngineClassConverterFactory);
        if (factory != null) innerOptions.Converters.Remove(factory);

        var deserialized = JsonSerializer.Deserialize<T>(ref reader, innerOptions);

        if (deserialized != null)
        {
            deserialized.ClassUrn = engineBase.ClassUrn;
            deserialized.Unique = engineBase.Unique;
            return deserialized;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var innerOptions = new JsonSerializerOptions(options);
        var factory = innerOptions.Converters.FirstOrDefault(c => c is EngineClassConverterFactory);
        if (factory != null) innerOptions.Converters.Remove(factory);

        JsonSerializer.Serialize(writer, (object)value, innerOptions);
    }
}