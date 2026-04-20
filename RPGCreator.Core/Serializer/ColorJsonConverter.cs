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

using Newtonsoft.Json;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Serializer;

public class ColorJsonConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToUnsignedInt());
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            try
            {
                uint colorValue = Convert.ToUInt32(reader.Value);
                return Color.FromUnsignedInt(colorValue);
            }
            catch (OverflowException)
            {
                // In this case, this is still the 'System.Drawing.Color' type, which is not any more supported.
                // So we do a fallback to a less efficient conversion, but at least it doesn't make the engine crash.
                // This is a temporary solution, and should be removed before the release of the engine.
                //
                // So if you see this message, and the engine was released... I forgot to remove it, shame on me.
                // Ward.
                var color = System.Drawing.Color.FromArgb(Convert.ToInt32(reader.Value));
                return new Color(color.R, color.G, color.B, color.A);
            }
        }

        throw new JsonSerializationException($"Unexpected token parsing Color. Expected Integer, got {reader.TokenType}.");
    }
}