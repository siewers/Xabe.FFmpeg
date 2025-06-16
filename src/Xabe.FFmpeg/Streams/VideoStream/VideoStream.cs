namespace Xabe.FFmpeg;

using Probe.Models;

/// <inheritdoc cref="IVideoStream" />
[PublicAPI]
internal sealed class VideoStream : StreamBase, IVideoStream, IFilterable
{
    private readonly Dictionary<string, string> _videoFilters = [];
    private string? _watermarkSource;

    internal VideoStream(string path, int index)
        : base(path, index)
    {
    }

    internal VideoStream(VideoStreamModel model, FormatModel formatModel)
        : base(model, formatModel)
    {
        Width = model.Width;
        Height = model.Height;
        Framerate = GetVideoFrameRate(model, Duration);
        Ratio = GetVideoAspectRatio(Width, Height);
        PixelFormat = model.PixelFormat;
        Rotation = model.Tags.Rotation;
    }

    internal ConversionParameters Parameters { get; } = [];

    /// <inheritdoc />
    public IEnumerable<IFilterConfiguration> GetFilters()
    {
        if (_videoFilters.Count != 0)
        {
            yield return new FilterConfiguration
                         {
                             FilterType = "-filter_complex",
                             StreamNumber = Index,
                             Filters = _videoFilters,
                         };
        }
    }

    /// <inheritdoc />
    public int Width { get; }

    /// <inheritdoc />
    public int Height { get; }

    /// <inheritdoc />
    public double Framerate { get; }

    /// <inheritdoc />
    public string? Ratio { get; }

    /// <inheritdoc />
    public string? PixelFormat { get; }

    /// <inheritdoc />
    public int? Rotation { get; }

    /// <inheritdoc />
    public override StreamType StreamType => StreamType.Video;

    /// <summary>
    ///     Create parameters string
    /// </summary>
    /// <param name="forPosition">Position for parameters</param>
    /// <returns>Parameters</returns>
    public override string BuildParameters(ParameterPosition forPosition)
    {
        var parameters = Parameters.Where(x => x.Position == forPosition).ToArray();
        return parameters.Length > 0
            ? string.Join(string.Empty, parameters.Select(x => x.Value))
            : string.Empty;
    }

    /// <inheritdoc />
    public IVideoStream ChangeSpeed(double multiplier)
    {
        _videoFilters["setpts"] = GetVideoSpeedFilter(multiplier);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream Rotate(RotateDegrees rotateDegrees)
    {
        var rotate = rotateDegrees == RotateDegrees.Invert
            ? "\"transpose=2,transpose=2\" "
            : $"\"transpose={(int)rotateDegrees}\" ";

        Parameters.AddPostInput("vf", rotate);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream Pad(int width, int height)
    {
        var parameter = $"\"scale={width}:{height}:force_original_aspect_ratio=decrease,pad={width}:{height}:-1:-1:color=black\"";
        Parameters.AddPostInput("vf", parameter);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream CopyStream()
    {
        return SetCodec(VideoCodec.copy);
    }

    /// <inheritdoc />
    public IVideoStream SetLoop(int count, int delay)
    {
        Parameters.AddPostInput("loop", count);

        if (delay > 0)
        {
            Parameters.AddPostInput("final_delay", delay / 100);
        }

        return this;
    }

    /// <inheritdoc />
    public IVideoStream AddSubtitles(MediaLocation subtitleLocation, VideoSize originalSize, string? characterEncoding, string? style)
    {
        return BuildSubtitleFilter(subtitleLocation, originalSize, characterEncoding, style);
    }

    /// <inheritdoc />
    public IVideoStream AddSubtitles(MediaLocation subtitleLocation, string? characterEncoding, string? style)
    {
        return BuildSubtitleFilter(subtitleLocation, originalSize: null, characterEncoding, style);
    }

    /// <inheritdoc />
    public IVideoStream Reverse()
    {
        Parameters.AddPostInput("vf", "reverse");
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetBitrate(long bitrate)
    {
        Parameters.AddPostInput("b:v", bitrate);
        return this;
    }

    public IVideoStream SetBitrate(long minBitrate, long maxBitrate, long bufferSize)
    {
        Parameters.AddPostInput("b:v", minBitrate);
        Parameters.AddPostInput("maxrate", maxBitrate);
        Parameters.AddPostInput("bufsize", bufferSize);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetFlags(params Flag[] flags)
    {
        return SetFlags(flags.Select(x => x.ToStringFast()).ToArray());
    }

    /// <inheritdoc />
    public IVideoStream SetFlags(params string[] flags)
    {
        var input = string.Join("+", flags);

        if (input[0] != '+')
        {
            input = "+" + input;
        }

        Parameters.AddPostInput("flags", input);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetFramerate(double framerate)
    {
        Parameters.AddPostInput("r", framerate.ToFFmpegFormat(3));
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetSize(VideoSize size)
    {
        Parameters.AddPostInput("s", size.ToStringFast(useMetadataAttributes: true));
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetSize(int width, int height)
    {
        Parameters.AddPostInput("s", $"{width}x{height}");
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetCodec(VideoCodec codec)
    {
        return SetCodec(codec.ToStringFast(useMetadataAttributes: true));
    }

    /// <inheritdoc />
    public IVideoStream SetCodec(string codec)
    {
        Parameters.AddPostInput("c:v", codec);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetBitstreamFilter(BitstreamFilter filter)
    {
        return SetBitstreamFilter(filter.ToStringFast(useMetadataAttributes: true));
    }

    /// <inheritdoc />
    public IVideoStream SetBitstreamFilter(string filter)
    {
        Parameters.AddPostInput("bsf:v", filter);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetSeek(TimeSpan seek)
    {
        if (seek > Duration)
        {
            throw new ArgumentException("Seek can not be greater than video duration. Seek: " + seek.TotalSeconds + " Duration: " + Duration.TotalSeconds);
        }

        Parameters.AddPreInput("ss", seek);

        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetOutputFramesCount(int number)
    {
        Parameters.AddPostInput("frames:v", number);
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetWatermark(string imagePath, Position position)
    {
        _watermarkSource = imagePath;

        var argument = position switch
        {
            Position.Bottom => "(main_w-overlay_w)/2:main_h-overlay_h",
            Position.Center => "x=(main_w-overlay_w)/2:y=(main_h-overlay_h)/2",
            Position.BottomLeft => "5:main_h-overlay_h",
            Position.UpperLeft => "5:5",
            Position.BottomRight => "(main_w-overlay_w):main_h-overlay_h",
            Position.UpperRight => "(main_w-overlay_w):5",
            Position.Left => "5:(main_h-overlay_h)/2",
            Position.Right => "(main_w-overlay_w-5):(main_h-overlay_h)/2",
            Position.Up => "(main_w-overlay_w)/2:5",
            _ => string.Empty,
        };

        _videoFilters["overlay"] = argument;
        return this;
    }

    /// <inheritdoc />
    public IVideoStream Split(TimeSpan startTime, TimeSpan duration)
    {
        Parameters.AddPostInput("ss", startTime);
        Parameters.AddPostInput("t", duration);
        return this;
    }

    /// <inheritdoc />
    public override IEnumerable<MediaLocation> GetSource()
    {
        return string.IsNullOrWhiteSpace(_watermarkSource)
            ? [Path]
            : [Path, _watermarkSource];
    }

    /// <inheritdoc />
    public IVideoStream SetInputFormat(Format inputFormat)
    {
        return SetInputFormat(inputFormat.ToStringFast(useMetadataAttributes: true));
    }

    /// <inheritdoc />
    public IVideoStream SetInputFormat(string? format)
    {
        if (format is not null)
        {
            Parameters.AddPreInput("f", format);
        }

        return this;
    }

    /// <inheritdoc />
    public IVideoStream UseNativeInputRead(bool readInputAtNativeFrameRate)
    {
        Parameters.AddPreInput("re");
        return this;
    }

    /// <inheritdoc />
    public IVideoStream SetStreamLoop(int loopCount)
    {
        Parameters.AddPreInput("stream_loop", loopCount);
        return this;
    }

    private static string GetVideoSpeedFilter(double multiplier)
    {
        if (multiplier is < 0.5 or > 2.0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Value has to be greater than 0.5 and less than 2.0.");
        }

        var videoMultiplier = multiplier >= 1 ? 1 - (multiplier - 1) / 2 : 1 + (multiplier - 1) * -2;
        return $"{videoMultiplier.ToFFmpegFormat()}*PTS ";
    }

    private VideoStream BuildSubtitleFilter(MediaLocation subtitleLocation, VideoSize? originalSize, string? characterEncoding, string? style)
    {
        var filter = $"'{subtitleLocation}'".Replace("\\", @"\\").Replace(":", "\\:");

        if (!string.IsNullOrEmpty(characterEncoding))
        {
            filter += $":charenc={characterEncoding}";
        }

        if (!string.IsNullOrEmpty(style))
        {
            filter += $":force_style=\'{style}\'";
        }

        if (originalSize.HasValue)
        {
            filter += $":original_size={originalSize.Value.ToStringFast(useMetadataAttributes: true)}";
        }

        _videoFilters.Add("subtitles", filter);
        return this;
    }

    public IVideoStream AddParameter(string parameter, ParameterPosition parameterPosition = ParameterPosition.PostInput)
    {
        Parameters.Add(ConversionParameter.Create(parameter, parameterPosition));
        return this;
    }

    private static double GetVideoFrameRate(VideoStreamModel videoStream, TimeSpan duration)
    {
        var frameCount = GetFrameCount(videoStream);
        var framerate = videoStream.RawFrameRate.Split('/');

        if (frameCount > 0)
        {
            return Math.Round(frameCount / duration.TotalSeconds, digits: 3);
        }

        return Math.Round(double.Parse(framerate[0]) / double.Parse(framerate[1]), digits: 3);
    }

    private static long GetFrameCount(StreamModelBase videoStream)
    {
        return long.TryParse(videoStream.NumberOfFrames, out var frameCount) ? frameCount : 0;
    }

    private static string GetVideoAspectRatio(int width, int height)
    {
        var cd = GetGcd(width, height);

        if (cd <= 0)
        {
            return "0:0";
        }

        return width / cd + ":" + height / cd;
    }

    /// <summary>
    ///     Gets the greatest common divisor of the two numbers.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private static int GetGcd(int width, int height)
    {
        while (width != 0 && height != 0)
        {
            if (width > height)
            {
                width -= height;
            }
            else
            {
                height -= width;
            }
        }

        return width == 0 ? height : width;
    }
}
