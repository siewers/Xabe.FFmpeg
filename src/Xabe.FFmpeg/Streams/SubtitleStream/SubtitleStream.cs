namespace Xabe.FFmpeg;

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Streams;
using Streams.SubtitleStream;

/// <inheritdoc />
[PublicAPI]
public sealed class SubtitleStream : ISubtitleStream
{
    private readonly ConversionParameters _parameters = [];

    internal SubtitleStream()
    {
    }

    /// <inheritdoc />
    public string Codec { get; internal set; }

    /// <inheritdoc />
    public string Path { get; internal set; }

    /// <inheritdoc />
    public string BuildParameters(ParameterPosition forPosition)
    {
        var parameters = _parameters.Where(x => x.Position == forPosition).ToArray();
        return parameters.Length > 0
            ? string.Join(string.Empty, parameters.Select(x => x.Parameter))
            : string.Empty;
    }

    /// <inheritdoc />
    public int Index { get; internal set; }

    /// <inheritdoc />
    public string Language { get; internal set; }

    /// <inheritdoc />
    public bool? IsDefault { get; internal set; }

    /// <inheritdoc />
    public bool? IsForced { get; internal set; }

    /// <inheritdoc />
    public string? Title { get; internal set; }

    /// <inheritdoc />
    public StreamType StreamType => StreamType.Subtitle;

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
    public IEnumerable<string> GetSource()
    {
        return [Path];
    }

    /// <inheritdoc />
    public ISubtitleStream SetCodec(SubtitleCodec codec)
    {
        return SetCodec(codec.ToString());
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
