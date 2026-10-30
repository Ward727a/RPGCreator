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
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Serializer.Converters;


public class ActorFeatureDataJsonConverter(ITypesRegistry typesRegistry) : JsonConverter<IActorFeatureData>
{
    private const string DiscriminatorPropertyName = "$type";

    public override IActorFeatureData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty(DiscriminatorPropertyName, out var typeProp))
        {
            Logger.Error("Missing '{0}' discriminator property on IActorFeatureData JSON object.", DiscriminatorPropertyName);
            return null;
        }

        var urnString = typeProp.GetString();
        if (string.IsNullOrEmpty(urnString))
            return null;

        var urn = new URN(urnString);
        var typeResult = typesRegistry.GetType(urn);
        if (typeResult.IsFailure)
        {
            Logger.Error("Unknown actor feature type URN '{0}': {1}", urn, typeResult.Error);
            return null;
        }

        var concreteType = typeResult.Value;
        return JsonSerializer.Deserialize(root.GetRawText(), concreteType, options) as IActorFeatureData;
    }

    public override void Write(Utf8JsonWriter writer, IActorFeatureData value, JsonSerializerOptions options)
    {
        var concreteType = value.GetType();
        var urnResult = typesRegistry.GetKey(concreteType);

        if (urnResult.IsFailure)
        {
            Logger.Error("Cannot serialize actor feature '{0}': missing [TypeDiscriminator] in ITypesRegistry.", concreteType.Name);
            writer.WriteNullValue();
            return;
        }

        var node = JsonSerializer.SerializeToNode(value, concreteType, options)?.AsObject() ?? new JsonObject();

        writer.WriteStartObject();
        writer.WriteString(DiscriminatorPropertyName, urnResult.Value.ToString());

        foreach (var kvp in node)
        {
            writer.WritePropertyName(kvp.Key);
            if (kvp.Value is null)
                writer.WriteNullValue();
            else
                kvp.Value.WriteTo(writer, options);
        }

        writer.WriteEndObject();
    }
}