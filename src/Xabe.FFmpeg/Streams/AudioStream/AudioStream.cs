namespace Xabe.FFmpeg;

using Probe.Models;

/// <inheritdoc cref="IAudioStream" />
[PublicAPI]
internal sealed class AudioStream : StreamBase, IAudioStream, IFilterable
{
    private readonly Dictionary<string, string> _audioFilters = [];
    private readonly ConversionParameters _parameters = [];

    internal AudioStream(AudioStreamModel streamModel, FormatModel formatModel)
        : base(streamModel, formatModel)
    {
        Channels = streamModel.Channels;
        ChannelLayout = streamModel.ChannelLayout;
        SampleRate = streamModel.SampleRate;
    }

    /// <inheritdoc />
    public int Channels { get; }

    /// <inheritdoc />
    public string? ChannelLayout { get; }

    /// <inheritdoc />
    public int SampleRate { get; }

    /// <inheritdoc />
    public override StreamType StreamType => StreamType.Audio;

    /// <inheritdoc />
    public IAudioStream Reverse()
    {
        _parameters.AddPostInput("af", "areverse");
        return this;
    }

    /// <inheritdoc />
    public override string BuildParameters(ParameterPosition forPosition)
    {
        var parameters = _parameters.Where(x => x.Position == forPosition).ToArray();
        return parameters.Length > 0
            ? string.Join(string.Empty, parameters.Select(x => x.Value))
            : string.Empty;
    }

    /// <inheritdoc />
    public IAudioStream Split(TimeSpan startTime, TimeSpan duration)
    {
        _parameters.AddPostInput("ss", startTime);
        _parameters.AddPostInput("t", duration);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream CopyStream()
    {
        return SetCodec(AudioCodec.copy);
    }

    /// <inheritdoc />
    public IAudioStream SetChannels(int channels)
    {
        _parameters.AddPostInput($"ac:{Index}", channels);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetBitstreamFilter(BitstreamFilter filter)
    {
        return SetBitstreamFilter(filter.ToStringFast(useMetadataAttributes: true));
    }

    /// <inheritdoc />
    public IAudioStream SetBitstreamFilter(string filter)
    {
        _parameters.AddPostInput("bsf:a", filter);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetBitrate(long bitRate)
    {
        _parameters.AddPostInput($"b:a:{Index}", bitRate);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetBitrate(long minBitrate, long maxBitrate, long bufferSize)
    {
        _parameters.AddPostInput($"b:a:{Index}", minBitrate);
        _parameters.AddPostInput("maxrate", maxBitrate);
        _parameters.AddPostInput("bufsize", bufferSize);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetSampleRate(int sampleRate)
    {
        _parameters.AddPostInput($"ar:{Index}", sampleRate);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream ChangeSpeed(double multiplier)
    {
        _audioFilters["atempo"] = $"{GetAudioSpeed(multiplier)}";
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetCodec(AudioCodec codec)
    {
        var codecString = codec switch
        {
            AudioCodec._4gv => "4gv",
            AudioCodec._8svx_exp => "8svx_exp",
            AudioCodec._8svx_fib => "8svx_fib",
            _ => codec.ToStringFast(),
        };

        return SetCodec(codecString);
    }

    /// <inheritdoc />
    public IAudioStream SetCodec(string codec)
    {
        _parameters.AddPostInput("c:a", codec);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetSeek(TimeSpan seek)
    {
        _parameters.AddPreInput("ss", seek);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetInputFormat(string inputFormat)
    {
        _parameters.AddPreInput("f", inputFormat);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetInputFormat(Format inputFormat)
    {
        return SetInputFormat(inputFormat.ToStringFast(useMetadataAttributes: true));
    }

    /// <inheritdoc />
    public IAudioStream UseNativeInputRead(bool readInputAtNativeFrameRate)
    {
        _parameters.AddPreInput("re");
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetStreamLoop(int loopCount)
    {
        _parameters.AddPreInput("stream_loop", loopCount);
        return this;
    }

    /// <inheritdoc />
    public IEnumerable<IFilterConfiguration> GetFilters()
    {
        if (_audioFilters.Count > 0)
        {
            yield return new FilterConfiguration
                         {
                             FilterType = "-filter:a",
                             StreamNumber = Index,
                             Filters = _audioFilters,
                         };
        }
    }

    private static string GetAudioSpeed(double multiplier)
    {
        if (multiplier is < 0.5 or > 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Value has to be greater than 0.5 and less than 2.0.");
        }

        return $"{multiplier.ToFFmpegFormat()} ";
    }
}
