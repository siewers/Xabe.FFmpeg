namespace Xabe.FFmpeg.Probe.Models;

using System.Text.Json.Serialization;

internal class ProbeModel(FormatModel format, StreamModelBase[] streams)
{
    [JsonPropertyName("format")]
    public FormatModel Format { get; } = format;

    [JsonPropertyName("streams")]
    public StreamModelBase[] Streams { get; } = streams;
}
