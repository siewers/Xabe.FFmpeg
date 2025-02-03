namespace Xabe.FFmpeg;

using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using Streams;

/// <inheritdoc cref="IAudioStream" />
[PublicAPI]
public sealed class AudioStream : IAudioStream, IFilterable
{
    private readonly Dictionary<string, string> _audioFilters = [];
    private readonly ConversionParameters _parameters = [];

    internal AudioStream()
    {
    }

    /// <inheritdoc />
    public IAudioStream Reverse()
    {
        _parameters.Add("af", "areverse");
        return this;
    }

    /// <inheritdoc />
    public string BuildParameters(ParameterPosition forPosition)
    {
        var parameters = _parameters.Where(x => x.Position == forPosition).ToArray();
        return parameters.Length > 0
            ? string.Join(string.Empty, parameters.Select(x => x.Parameter))
            : string.Empty;
    }

    /// <inheritdoc />
    public IAudioStream Split(TimeSpan startTime, TimeSpan duration)
    {
        _parameters.Add("ss", startTime);
        _parameters.Add("t", duration);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream CopyStream()
    {
        return SetCodec(AudioCodec.copy);
    }

    /// <inheritdoc />
    public StreamType StreamType => StreamType.Audio;

    /// <inheritdoc />
    public IAudioStream SetChannels(int channels)
    {
        _parameters.Add($"ac:{Index}", channels);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetBitstreamFilter(BitstreamFilter filter)
    {
        return SetBitstreamFilter($"{filter}");
    }

    /// <inheritdoc />
    public IAudioStream SetBitstreamFilter(string filter)
    {
        _parameters.Add("bsf:a", filter);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetBitrate(long bitRate)
    {
        _parameters.Add($"b:a:{Index}", bitRate);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetBitrate(long minBitrate, long maxBitrate, long bufferSize)
    {
        _parameters.Add($"b:a:{Index}", minBitrate);
        _parameters.Add("maxrate", maxBitrate);
        _parameters.Add("bufsize", bufferSize);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetSampleRate(int sampleRate)
    {
        _parameters.Add($"ar:{Index}", sampleRate);
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
            _ => codec.ToString(),
        };

        return SetCodec(codecString);
    }

    /// <inheritdoc />
    public IAudioStream SetCodec(string codec)
    {
        _parameters.Add("c:a", codec);
        return this;
    }

    /// <inheritdoc />
    public int Index { get; internal set; }

    /// <inheritdoc />
    public TimeSpan Duration { get; internal set; }

    /// <inheritdoc />
    public string Codec { get; internal set; }

    /// <inheritdoc />
    public long Bitrate { get; internal set; }

    /// <inheritdoc />
    public int Channels { get; internal set; }

    /// <inheritdoc />
    public string? ChannelLayout { get; internal set; }

    /// <inheritdoc />
    public int SampleRate { get; internal set; }

    /// <inheritdoc />
    public string Language { get; internal set; }

    /// <inheritdoc />
    public string? Title { get; internal set; }

    /// <inheritdoc />
    public bool? IsDefault { get; internal set; }

    /// <inheritdoc />
    public bool? IsForced { get; internal set; }

    /// <inheritdoc />
    public IEnumerable<string> GetSource()
    {
        return [Path];
    }

    /// <inheritdoc />
    public string Path { get; set; }

    /// <inheritdoc />
    public IAudioStream SetSeek(TimeSpan seek)
    {
        _parameters.Add("ss", seek.ToFFmpeg(), ParameterPosition.PreInput);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetInputFormat(string inputFormat)
    {
        _parameters.Add("f", inputFormat, ParameterPosition.PreInput);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetInputFormat(Format inputFormat)
    {
        return SetInputFormat(inputFormat.ToString());
    }

    /// <inheritdoc />
    public IAudioStream UseNativeInputRead(bool readInputAtNativeFrameRate)
    {
        _parameters.Add("re", ParameterPosition.PreInput);
        return this;
    }

    /// <inheritdoc />
    public IAudioStream SetStreamLoop(int loopCount)
    {
        _parameters.Add("stream_loop", loopCount, ParameterPosition.PreInput);
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
