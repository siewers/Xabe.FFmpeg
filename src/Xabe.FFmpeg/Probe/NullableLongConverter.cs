namespace Xabe.FFmpeg;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class NullableLongConverter : JsonConverter<long?>
{
    /// <inheritdoc />
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Number
            ? reader.GetInt64()
            : long.TryParse(reader.GetString(), out var bitRate)
                ? bitRate
                : null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
    }
}
