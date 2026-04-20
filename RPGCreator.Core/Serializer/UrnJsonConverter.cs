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
using Newtonsoft.Json.Linq;
using RPGCreator.SDK.Types;
using Logger = RPGCreator.SDK.Logging.Logger;

namespace RPGCreator.Core.Serializer;

public class UrnJsonConverter : JsonConverter<URN>
{
    public override void WriteJson(JsonWriter writer, URN value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override URN ReadJson(JsonReader reader, Type objectType, URN existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        string? urnString = reader.Value?.ToString();
        if (string.IsNullOrEmpty(urnString))
        {
            try
            {
                var urnObject = JObject.Load(reader);


                if (urnObject is JObject jObject)
                {
                    urnString = jObject.GetValue("FullName")?.ToString() ?? "";
                }
            }
            catch
            {
                // Ignore exceptions and return URN.Empty
                Logger.Error("Failed to read URN from JSON object.");
            }
        }
        
        if (!string.IsNullOrEmpty(urnString))
        {
            return URN.Parse(urnString);
        }
        return URN.Empty;
    }
}