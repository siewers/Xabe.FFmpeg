namespace Xabe.FFmpeg;

using Probe.Models;

public abstract class StreamBase : IStream
{
    protected StreamBase(string path, int index)
    {
        Path = path;
        Index = index;
    }

    internal StreamBase(StreamModelBase streamModel, FormatModel formatModel)
    {
        var duration = GetStreamDuration(streamModel, formatModel);

        Path = formatModel.FileName;
        Title = formatModel.Tags.Title;
        Codec = streamModel.CodecName;
        Language = streamModel.Tags.Language;
        Duration = duration;
        Index = streamModel.Index;
        Bitrate = Math.Abs(streamModel.Tags.BitRate ?? formatModel.BitRate ?? 0);
        IsDefault = streamModel.Disposition.IsDefault;
        IsForced = streamModel.Disposition.IsForced;
    }

    public string Path { get; }

    public string? Title { get; }

    public int Index { get; }

    public string? Codec { get; }

    public string? Language { get; }

    public bool? IsDefault { get; }

    public bool? IsForced { get; }

    public TimeSpan Duration { get; }

    public long Bitrate { get; }

    public abstract StreamType StreamType { get; }

    public abstract string BuildParameters(ParameterPosition forPosition);

    public virtual IEnumerable<string> GetSource()
    {
        return [Path];
    }

    private static TimeSpan GetStreamDuration(StreamModelBase streamModel, FormatModel formatModel)
    {
        return streamModel.Duration ?? streamModel.Tags.Duration ?? formatModel.Duration;
    }
}
