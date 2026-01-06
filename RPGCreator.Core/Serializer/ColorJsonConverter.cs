using System.Drawing;
using Newtonsoft.Json;

namespace RPGCreator.Core.Serializer;

public class ColorJsonConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToArgb());
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            return Color.FromArgb(Convert.ToInt32(reader.Value));
        }
        
        return Color.Empty; 
    }
}