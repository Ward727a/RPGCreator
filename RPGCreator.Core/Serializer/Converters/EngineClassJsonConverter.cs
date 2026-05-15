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
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.Core.Serializer;

public class EngineClassJsonConverter : JsonConverter<IEngineClass>
{
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<EngineClassJsonConverter>();

    public override IEngineClass? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!ClassesRegistry.HasType(typeToConvert)) return null;

        var classInstanceResult = ClassesRegistry.GetEngineClass(typeToConvert).Value.Factory.DefaultConstructor();
        if (classInstanceResult.IsFailure)
        {
            Logger.Error("Failed to create default instance of {Type}: {Error}", args: [typeToConvert, classInstanceResult.Error]);
            return null;
        }

        var engineBase = (IEngineClass)classInstanceResult.Value!;

        var innerOptions = new JsonSerializerOptions(options);
        innerOptions.Converters.Remove(this);

        if (JsonSerializer.Deserialize(ref reader, typeToConvert, innerOptions) is IEngineClass deserialized)
        {
            deserialized.ClassUrn = engineBase.ClassUrn;
            deserialized.Unique = engineBase.Unique;
            return deserialized;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, IEngineClass value, JsonSerializerOptions options)
    {
        var innerOptions = new JsonSerializerOptions(options);
        innerOptions.Converters.Remove(this);

        JsonSerializer.Serialize(writer, (object)value, innerOptions);
    }
}