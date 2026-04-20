
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RPGCreator.Core.Serializer;

public class UlidJsonConverter : JsonConverter<Ulid>
{
    public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return Ulid.Parse(reader.GetString());
        }

        return Ulid.Empty;
    }

    public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}