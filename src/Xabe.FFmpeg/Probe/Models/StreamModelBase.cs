namespace Xabe.FFmpeg;

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
    public string CodecName { get; set; }

    [JsonPropertyName("codec_long_name")]
    public string CodecLongName { get; set; }

    public string r_frame_rate { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration { get; set; }

    public string pix_fmt { get; set; }

    [JsonPropertyName("tags")]
    public TagsModel Tags { get; set; } = new();

    public string nb_frames { get; set; }

    [JsonPropertyName("disposition")]
    public StreamDispositionModel Disposition { get; set; }
}
