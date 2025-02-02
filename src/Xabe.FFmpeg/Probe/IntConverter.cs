namespace Xabe.FFmpeg;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class IntConverter : JsonConverter<int>
{
    /// <inheritdoc />
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Number ? reader.GetInt32() : int.Parse(reader.GetString());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
    }
}
