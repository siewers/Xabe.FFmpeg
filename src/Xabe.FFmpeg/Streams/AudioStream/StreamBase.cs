namespace Xabe.FFmpeg;

using Probe.Models;

internal abstract class StreamBase(string path, int index) : IStream
{
    internal StreamBase(StreamModelBase streamModel, FormatModel formatModel)
        : this(formatModel.FileName, streamModel.Index)
    {
        Title = formatModel.Tags.Title;
        Codec = streamModel.CodecName;
        Language = streamModel.Tags.Language;
        Duration = streamModel.Duration ?? streamModel.Tags.Duration ?? formatModel.Duration;
        Bitrate = Math.Abs(streamModel.Bitrate ?? streamModel.Tags.Bitrate ?? formatModel.Bitrate ?? 0);
        IsForced = streamModel.Disposition.IsForced;
        IsDefault = streamModel.Disposition.IsDefault;
    }

    public string Path { get; } = path;

    public string? Title { get; }

    public int Index { get; } = index;

    public string? Codec { get; }

    public string? Language { get; }

    public TimeSpan Duration { get; }

    public long Bitrate { get; }

    public bool? IsForced { get; }

    public bool? IsDefault { get; }

    public abstract StreamType StreamType { get; }

    public abstract string BuildParameters(ParameterPosition forPosition);

    public virtual IEnumerable<MediaLocation> GetSource()
    {
        return [Path];
    }
}
