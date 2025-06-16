namespace Xabe.FFmpeg;

using Probe.Models;

[PublicAPI]
internal sealed class SubtitleStream : StreamBase, ISubtitleStream
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
            ? string.Join(string.Empty, parameters.Select(x => x.Value))
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

        _parameters.AddPostInput($"metadata:s:s:{Index}", $"language={language}");

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
        _parameters.AddPostInput("c:s", codec);
        return this;
    }

    /// <inheritdoc />
    public ISubtitleStream UseNativeInputRead(bool readInputAtNativeFrameRate)
    {
        _parameters.AddPreInput("re");
        return this;
    }

    /// <inheritdoc />
    public ISubtitleStream SetStreamLoop(int loopCount)
    {
        _parameters.AddPreInput("stream_loop", loopCount);
        return this;
    }
}
