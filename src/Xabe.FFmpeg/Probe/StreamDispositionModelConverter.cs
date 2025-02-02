namespace Xabe.FFmpeg;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class StreamDispositionModelConverter : JsonConverter<StreamDispositionModel>
{
    /// <inheritdoc />
    public override StreamDispositionModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDisposition = JsonElement.ParseValue(ref reader);
        return StreamDispositionModel.CreateInstance(jsonDisposition);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, StreamDispositionModel value, JsonSerializerOptions options)
    {
    }
}
