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
using RPGCreator.Shared.Extensions;

namespace RPGCreator.EngineLib.Serializer.Converters;

public class UInt128JsonConverter : JsonConverter<UInt128>
{
    public override UInt128 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        ulong upper = 0, lower = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new UInt128(upper, lower);

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");

            string propertyName = reader.GetString() ?? throw new JsonException("Property name cannot be null");

            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON");

            switch (propertyName)
            {
                case "Upper":
                    upper = reader.GetUInt64();
                    break;
                case "Lower":
                    lower = reader.GetUInt64();
                    break;
                default:
                    throw new JsonException($"Unexpected property: {propertyName}");
            }
        }

        throw new JsonException("Unexpected end of JSON");
        
    }

    public override void Write(Utf8JsonWriter writer, UInt128 value, JsonSerializerOptions options)
    {
        
        writer.WriteStartObject();
        writer.WriteNumber("Upper", value.GetUpper());
        writer.WriteNumber("Lower", value.GetLower());
        writer.WriteEndObject();
    }
}