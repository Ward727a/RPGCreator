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
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Serializer.Converters;

public class TileIndexJsonConverter : JsonConverter<TileIndex>
{
    public override TileIndex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");
        
        var values = new int[3];
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return TileIndex.Make(values[0], values[1], values[2]);

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName token");

            string propertyName = reader.GetString() ?? throw new JsonException("Property name cannot be null");

            if (!reader.Read())
                throw new JsonException("Unexpected end of JSON");

            switch (propertyName)
            {
                case "X":
                    if(!reader.TryGetInt32(out values[0]))
                        throw new JsonException("Expected integer value for X");
                    break;
                case "Y":
                    if(!reader.TryGetInt32(out values[1]))
                        throw new JsonException("Expected integer value for Y");
                    break;
                case "TilesByRows":
                    if(!reader.TryGetInt32(out values[2]))
                        throw new JsonException("Expected integer value for TilesByRows");
                    break;
                default:
                    throw new JsonException($"Unexpected property: {propertyName}");
            }
        }


        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, TileIndex value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("TilesByRows", value.TilesByRows);
        writer.WriteEndObject();
    }
}