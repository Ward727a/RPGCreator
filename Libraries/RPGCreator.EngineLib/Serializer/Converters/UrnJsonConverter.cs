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
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Serializer.Converters;

public class UrnJsonConverter : JsonConverter<URN>
{

    public override URN Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var urnString = reader.GetString();

            if (string.IsNullOrEmpty(urnString))
            {
                return URN.Empty;
            }

            return URN.Parse(urnString);
        }

        return URN.Empty;
    }

    public override void Write(Utf8JsonWriter writer, URN value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}