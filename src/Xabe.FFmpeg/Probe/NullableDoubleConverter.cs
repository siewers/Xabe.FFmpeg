namespace Xabe.FFmpeg;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class NullableDoubleConverter : JsonConverter<double?>
{
    public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Number
            ? reader.GetDouble()
            : double.TryParse(reader.GetString(), out var value)
                ? value
                : null;
    }

    public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
    {
    }
}
