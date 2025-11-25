using Newtonsoft.Json;

namespace RPGCreator.Core.Serializer;

public class UlidJsonConverter : JsonConverter<Ulid>
{
    public override void WriteJson(JsonWriter writer, Ulid value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override Ulid ReadJson(JsonReader reader, Type objectType, Ulid existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if(reader.TokenType == JsonToken.String)
        {
            var ulidString = (string)reader.Value!;
            return Ulid.Parse(ulidString);
        }

        return Ulid.Empty;
    }
}