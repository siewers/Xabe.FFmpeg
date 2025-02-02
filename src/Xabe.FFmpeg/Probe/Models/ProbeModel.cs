namespace Xabe.FFmpeg;

using System.Text.Json.Serialization;

internal class ProbeModel
{
    public ProbeModel(FormatModel format, StreamModelBase[] streams)
    {
        Format = format;
        Streams = streams ?? [];
    }

    [JsonPropertyName("format")]
    public FormatModel Format { get; }

    [JsonPropertyName("streams")]
    public StreamModelBase[] Streams { get; }
}
