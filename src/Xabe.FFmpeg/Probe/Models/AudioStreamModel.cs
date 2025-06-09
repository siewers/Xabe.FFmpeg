namespace Xabe.FFmpeg.Probe.Models;

using System.Text.Json.Serialization;

internal sealed class AudioStreamModel : StreamModelBase
{
    [JsonPropertyName("channels")]
    public int Channels { get; set; }

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; }

    [JsonPropertyName("channel_layout")]
    public string? ChannelLayout { get; set; }

    [JsonPropertyName("bit_rate")]
    public long? BitRate { get; set; }
}
