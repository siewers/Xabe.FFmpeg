namespace Xabe.FFmpeg;

using Probe.Models;

[PublicAPI]
public sealed class SubtitleStream : StreamBase, ISubtitleStream
{
    private readonly ConversionParameters _parameters = [];

    internal SubtitleStream(StreamModelBase streamModel, FormatModel formatModel)
        : base(streamModel, formatModel)
    {
    }

    /// <inheritdoc />
    public override StreamType StreamType => StreamType.Subtitle;

    /// <inheritdoc />
    public override string BuildParameters(ParameterPosition forPosition)
    {
        var parameters = _parameters.Where(x => x.Position == forPosition).ToArray();
        return parameters.Length > 0
            ? string.Join(string.Empty, parameters.Select(x => x.Parameter))
            : string.Empty;
    }

    /// <inheritdoc />
    public ISubtitleStream SetLanguage(string? lang)
    {
        var language = !string.IsNullOrEmpty(lang) ? lang : Language;

        if (string.IsNullOrEmpty(language))
        {
            return this;
        }

        _parameters.Add($"metadata:s:s:{Index}", $"language={language}");

        return this;
    }

    /// <inheritdoc />
    public ISubtitleStream SetCodec(SubtitleCodec codec)
    {
        return SetCodec(codec.ToStringFast());
    }

    /// <inheritdoc />
    public ISubtitleStream SetCodec(string codec)
    {
        _parameters.Add("c:s", codec);
        return this;
    }

    /// <inheritdoc />
    public ISubtitleStream UseNativeInputRead(bool readInputAtNativeFrameRate)
    {
        _parameters.Add("re", ParameterPosition.PreInput);
        return this;
    }

    /// <inheritdoc />
    public ISubtitleStream SetStreamLoop(int loopCount)
    {
        _parameters.Add("stream_loop", loopCount, ParameterPosition.PreInput);
        return this;
    }
}
