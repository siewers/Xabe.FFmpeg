namespace Xabe.FFmpeg.Probe.Models;

using System;
using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "codec_type")]
[JsonDerivedType(typeof(AudioStreamModel), "audio")]
[JsonDerivedType(typeof(VideoStreamModel), "video")]
[JsonDerivedType(typeof(SubtitleStreamModel), "subtitle")]
[JsonDerivedType(typeof(DataStreamModel), "data")]
internal class StreamModelBase
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("codec_name")]
    public required string CodecName { get; set; }

    [JsonPropertyName("codec_long_name")]
    public string? CodecLongName { get; set; }

    [JsonPropertyName("r_frame_rate")]
    public required string RawFrameRate { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration { get; set; }

    [JsonPropertyName("pix_fmt")]
    public required string PixelFormat { get; set; }

    [JsonPropertyName("tags")]
    public TagsModel Tags { get; set; } = new();

    [JsonPropertyName("nb_frames")]
    public string? NumberOfFrames { get; set; }

    [JsonPropertyName("disposition")]
    public required StreamDispositionModel Disposition { get; set; }
}
