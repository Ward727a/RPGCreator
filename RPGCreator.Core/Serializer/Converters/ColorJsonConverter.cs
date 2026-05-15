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
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Serializer;

public class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            try
            {
                var colorValue = reader.GetUInt32();
                return Color.FromUnsignedInt(colorValue);
            }
            catch (FormatException fExc)
            {
                Logger.Error(fExc, "Failed to parse Color from JSON. Bad format (maybe it has a decimal point?).");
                return Color.Black;
            }
        }
        
        Logger.Error("Unexpected token parsing Color. Expected Number, got {reader.TokenType}.", reader.TokenType);
        return Color.Black;
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnsignedInt());
    }
}