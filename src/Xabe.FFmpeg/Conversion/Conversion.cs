namespace Xabe.FFmpeg;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Events;

/// <inheritdoc />
[PublicAPI]
public partial class Conversion : IConversion
{
    private readonly Lock _builderLock = new();
    private readonly Dictionary<string, int> _inputFileMap = [];
    private readonly ConversionParameters _parameters = [];
    private readonly List<IStream> _streams = [];
    private readonly Dictionary<ParameterPosition, List<string>> _userDefinedParameters = [];
    private Func<string, MediaLocation>? _buildInputFileName;
    private Func<string, MediaLocation>? _buildOutputFileName;
    private FFmpegWrapper? _ffmpeg;
    private bool _hasInputBuilder;
    private MediaLocation _output;
    private ProcessPriorityClass? _priority;

    public Conversion()
    {
        _userDefinedParameters[ParameterPosition.PostInput] = [];
        _userDefinedParameters[ParameterPosition.PreInput] = [];
    }

    /// <inheritdoc />
    public event ConversionProgressEventHandler? OnProgress;

    /// <inheritdoc />
    public event DataReceivedEventHandler? OnDataReceived;

    /// <inheritdoc />
    public event VideoDataEventHandler? OnVideoDataReceived;

    /// <inheritdoc />
    public MediaLocation OutputFilePath { get; private set; }

    /// <inheritdoc />
    public PipeDescriptor? OutputPipeDescriptor { get; private set; }

    /// <inheritdoc />
    public IEnumerable<IStream> Streams => _streams;

    /// <inheritdoc />
    public Task<IConversionResult> Start(CancellationToken cancellationToken = default)
    {
        var parameters = Build();
        return Start(parameters, cancellationToken);
    }

    /// <inheritdoc />
    public string Build()
    {
        lock (_builderLock)
        {
            var builder = new StringBuilder();

            _buildOutputFileName ??= _ => _output;

            builder.Append(string.Join(" ", _userDefinedParameters[ParameterPosition.PreInput].Select(x => x.Trim())) + " ");
            AppendParameters(builder, ParameterPosition.PreInput);
            AppendStreamsPreInputs(builder);

            if (_buildInputFileName is not null)
            {
                _hasInputBuilder = true;
                builder.Append(_buildInputFileName("_%03d"));
            }

            AppendInputs(builder);
            AppendStreamsPostInputs(builder);
            AppendFilters(builder);
            AddStreamMappings(builder);
            AppendParameters(builder, ParameterPosition.PostInput);
            builder.Append(string.Join(" ", _userDefinedParameters[ParameterPosition.PostInput].Select(x => x.Trim())) + " ");

            var outputFilename = _buildOutputFileName?.Invoke("_%03d").Escape();
            builder.Append(outputFilename);

            return builder.ToString();
        }
    }

    /// <inheritdoc />
    public Task<IConversionResult> Start(string parameters)
    {
        return Start(parameters, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IConversionResult> Start(string parameters, CancellationToken cancellationToken)
    {
        if (_ffmpeg is not null)
        {
            throw new InvalidOperationException("Conversion has already been started.");
        }

        _ffmpeg = new FFmpegWrapper();

        try
        {
            _ffmpeg.OnProgress += OnProgress;
            _ffmpeg.OnDataReceived += OnDataReceived;
            _ffmpeg.OnVideoDataReceived += OnVideoDataReceived;
            CreateOutputDirectoryIfNotExists();
            var startTime = Stopwatch.GetTimestamp();
            await _ffmpeg.RunProcess(parameters, _priority, cancellationToken);
            var endTime = Stopwatch.GetTimestamp();

            return new ConversionResult
                   {
                       StartTime = new DateTime(startTime),
                       EndTime = new DateTime(endTime),
                       Duration = Stopwatch.GetElapsedTime(startTime, endTime),
                       Arguments = parameters,
                       OutputLog = string.Join(Environment.NewLine, _ffmpeg.OutputLog),
                   };
        }
        finally
        {
            _ffmpeg.OnProgress -= OnProgress;
            _ffmpeg.OnDataReceived -= OnDataReceived;
            _ffmpeg.OnVideoDataReceived -= OnVideoDataReceived;
            _ffmpeg = null;
        }
    }

    /// <inheritdoc />
    public IConversion AddParameter(string parameter, ParameterPosition parameterPosition = ParameterPosition.PostInput)
    {
        _userDefinedParameters[parameterPosition].Add(parameter);
        return this;
    }

    /// <inheritdoc />
    public IConversion AddStream<T>(T? stream) where T : IStream
    {
        if (stream is not null)
        {
            _streams.Add(stream);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion AddStreams(IEnumerable<IStream?> streams)
    {
        foreach (var stream in streams)
        {
            AddStream(stream);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetHashFormat(Hash hashFormat = Hash.SHA256)
    {
        SetOutputFormat(Format.hash);
        var format = hashFormat.ToStringFast(useMetadataAttributes: true);
        return SetHashFormat(format);
    }

    /// <inheritdoc />
    public IConversion SetHashFormat(string hashFormat)
    {
        _parameters.AddPostInput("hash", hashFormat);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetPreset(ConversionPreset preset)
    {
        _parameters.AddPostInput("preset", preset.ToStringFast().ToLower());
        return this;
    }

    /// <inheritdoc />
    public IConversion SetSeek(TimeSpan? seek)
    {
        if (seek.HasValue)
        {
            _parameters.AddPostInput("ss", seek.Value);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetInputTime(TimeSpan? time)
    {
        if (time.HasValue)
        {
            _parameters.AddPreInput("t", time.Value);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetOutputTime(TimeSpan? time)
    {
        if (time.HasValue)
        {
            _parameters.AddPostInput("t", time.Value);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion UseMultiThread(bool multiThread)
    {
        var threads = multiThread ? Environment.ProcessorCount : 1;
        _parameters.AddPostInput("threads", Math.Min(threads, val2: 16));
        return this;
    }

    /// <inheritdoc />
    public IConversion UseMultiThread(int threadsCount)
    {
        _parameters.AddPostInput("threads", threadsCount);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetOutput(MediaLocation mediaLocation)
    {
        OutputFilePath = mediaLocation;
        _output = mediaLocation;
        return this;
    }

    /// <inheritdoc />
    public IConversion PipeOutput(PipeDescriptor descriptor = PipeDescriptor.stdout)
    {
        SetOutput($"pipe:{descriptor.ToStringFast()}");
        OutputPipeDescriptor = descriptor;
        return this;
    }

    /// <inheritdoc />
    public IConversion SetVideoBitrate(long bitrate)
    {
        _parameters.AddPostInput("b:v", bitrate);
        _parameters.AddPostInput("minrate", bitrate);
        _parameters.AddPostInput("maxrate", bitrate);
        _parameters.AddPostInput("bufsize", bitrate);

        if (HasH264Stream())
        {
            _parameters.AddPostInput("x264opts", "nal-hrd=cbr:force-cfr=1");
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetAudioBitrate(long bitrate)
    {
        _parameters.AddPostInput("b:a", bitrate);
        return this;
    }

    /// <inheritdoc />
    public IConversion UseShortest(bool useShortest)
    {
        if (useShortest)
        {
            _parameters.AddPostInput("shortest", useShortest);
        }
        else
        {
            _parameters.Remove("shortest");
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetPriority(ProcessPriorityClass? priority)
    {
        _priority = priority;
        return this;
    }

    /// <inheritdoc />
    public IConversion ExtractEveryNthFrame(int frameNo, Func<string, MediaLocation> buildOutputFileName)
    {
        _buildOutputFileName = buildOutputFileName;
        OutputFilePath = buildOutputFileName("");
        _parameters.AddPostInput("vf", $"select='not(mod(n\\,{frameNo}))'");
        SetVideoSyncMethod(VideoSyncMethod.vfr);

        return this;
    }

    /// <inheritdoc />
    public IConversion ExtractNthFrame(int frameNo, Func<string, MediaLocation> buildOutputFileName)
    {
        _buildOutputFileName = buildOutputFileName;
        _parameters.AddPostInput("vf", $"select='eq(n\\,{frameNo})'");
        OutputFilePath = buildOutputFileName("");
        SetVideoSyncMethod(VideoSyncMethod.passthrough);
        return this;
    }

    /// <inheritdoc />
    public IConversion BuildVideoFromImages(int startNumber, Func<string, MediaLocation> buildInputFileName)
    {
        _buildInputFileName = buildInputFileName;
        _parameters.AddPreInput("start_number", startNumber);
        return this;
    }

    /// <inheritdoc />
    public IConversion BuildVideoFromImages(IEnumerable<MediaLocation> imageFiles)
    {
        var builder = new InputBuilder();
        _buildInputFileName = builder.PrepareInputFiles(imageFiles, out _);

        return this;
    }

    /// <inheritdoc />
    public IConversion SetInputFramerate(double framerate)
    {
        _parameters.AddPreInput("framerate", framerate.ToFFmpegFormat(3));
        _parameters.AddPreInput("r", framerate.ToFFmpegFormat(3));
        return this;
    }

    /// <inheritdoc />
    public IConversion SetFramerate(double framerate)
    {
        _parameters.AddPostInput("framerate", framerate.ToFFmpegFormat(3));
        _parameters.AddPostInput("r", framerate.ToFFmpegFormat(3));
        return this;
    }

    /// <inheritdoc />
    public IConversion UseHardwareAcceleration(HardwareAccelerator hardwareAccelerator, VideoCodec decoder, VideoCodec encoder, int device = 0)
    {
        return UseHardwareAcceleration(hardwareAccelerator.ToStringFast(), decoder.ToStringFast(), encoder.ToStringFast(), device);
    }

    /// <inheritdoc />
    public IConversion UseHardwareAcceleration(string hardwareAccelerator, string decoder, string encoder, int device = 0)
    {
        _parameters.AddPreInput("hwaccel", hardwareAccelerator);
        _parameters.AddPreInput("c:v", decoder);
        _parameters.AddPostInput("c:v", encoder);

        if (device != 0)
        {
            _parameters.AddPreInput("hwaccel_device", device);
        }

        UseMultiThread(false);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetOverwriteOutput(bool overwrite)
    {
        if (overwrite)
        {
            _parameters.AddPostInput("y");
            _parameters.Remove("n");
        }
        else
        {
            _parameters.AddPostInput("n");
            _parameters.Remove("y");
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetInputFormat(Format inputFormat)
    {
        var format = inputFormat.ToStringFast(useMetadataAttributes: true);
        return SetInputFormat(format);
    }

    /// <inheritdoc />
    public IConversion SetInputFormat(string? format)
    {
        if (format is not null)
        {
            _parameters.AddPreInput("f", format);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetOutputFormat(Format outputFormat)
    {
        var format = outputFormat.ToStringFast(useMetadataAttributes: true);
        return SetOutputFormat(format);
    }

    /// <inheritdoc />
    public IConversion SetOutputFormat(string? format)
    {
        if (format is not null)
        {
            _parameters.AddPostInput("f", format);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetPixelFormat(PixelFormat pixelFormat)
    {
        var format = pixelFormat.ToStringFast(true);

        return SetPixelFormat(format);
    }

    /// <inheritdoc />
    public IConversion SetPixelFormat(string? pixelFormat)
    {
        if (pixelFormat is not null)
        {
            _parameters.AddPostInput("pix_fmt", pixelFormat);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetVideoSyncMethod(VideoSyncMethod method)
    {
        if (method == VideoSyncMethod.auto)
        {
            _parameters.AddPostInput("vsync", value: -1);
        }
        else
        {
            _parameters.AddPostInput("vsync", method.ToStringFast());
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion AddDesktopStream(string? videoSize = null, double framerate = 30, int xOffset = 0, int yOffset = 0)
    {
        var (path, format) = GetPathAndFormat();
        var index = _streams.Count != 0 ? _streams.Max(x => x.Index) + 1 : 0;

        var stream = new VideoStream(path, index);

        stream.SetInputFormat(format);

        stream.Parameters.AddPreInput("framerate", framerate.ToFFmpegFormat(4));
        stream.Parameters.AddPreInput("offset_x", xOffset);
        stream.Parameters.AddPreInput("offset_y", yOffset);

        if (videoSize is not null)
        {
            stream.Parameters.AddPreInput("video_size", videoSize);
        }

        AddStream(stream);

        return this;

        (string Path, Format Format) GetPathAndFormat()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return (Path: "desktop", Format: Format.gdigrab);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return (Path: "1:1", Format: Format.avfoundation);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return (Path: ":0.0+0,0", Format: Format.x11grab);
            }

            throw new PlatformNotSupportedException();
        }
    }

    /// <inheritdoc />
    public IConversion AddDesktopStream(VideoSize videoSize, double framerate = 30, int xOffset = 0, int yOffset = 0)
    {
        return AddDesktopStream(videoSize.ToStringFast(useMetadataAttributes: true), framerate, xOffset, yOffset);
    }

    private void CreateOutputDirectoryIfNotExists()
    {
        if (string.IsNullOrWhiteSpace(OutputFilePath) || OutputPipeDescriptor is not null)
        {
            return;
        }

        try
        {
            var directoryName = Path.GetDirectoryName(OutputFilePath);

            if (directoryName is not null)
            {
                Directory.CreateDirectory(directoryName);
            }
        }
        catch (IOException)
        {
        }
    }

    private void AppendStreamsPostInputs(StringBuilder builder)
    {
        foreach (var stream in _streams)
        {
            builder.Append(stream.BuildParameters(ParameterPosition.PostInput));
        }
    }

    private void AppendStreamsPreInputs(StringBuilder builder)
    {
        foreach (var stream in _streams)
        {
            builder.Append(stream.BuildParameters(ParameterPosition.PreInput));
        }
    }

    private void AppendFilters(StringBuilder builder)
    {
        var configurations = new List<IFilterConfiguration>();

        configurations.AddRange(_streams.OfType<IFilterable>().SelectMany(filterable => filterable.GetFilters()));

        var filterGroups = configurations.GroupBy(configuration => configuration.FilterType);

        foreach (var filterGroup in filterGroups)
        {
            builder.Append($"{filterGroup.Key} \"");

            foreach (var configuration in configurations.Where(x => x.FilterType == filterGroup.Key))
            {
                var values = new List<string>();

                foreach (var filter in configuration.Filters)
                {
                    var map = $"[{configuration.StreamNumber}]";
                    var value = string.IsNullOrEmpty(filter.Value) ? $"{filter.Key} " : $"{filter.Key}={filter.Value}";
                    values.Add($"{map} {value} ");
                }

                builder.Append(string.Join(";", values));
            }

            builder.Append("\" ");
        }
    }

    /// <summary>
    ///     Create map for included streams, including the InputBuilder if required
    /// </summary>
    /// <returns>Map argument</returns>
    private void AddStreamMappings(StringBuilder builder)
    {
        foreach (var stream in _streams)
        {
            if (_hasInputBuilder) // If we have an input builder we always want to map the first video stream as it will be created by our input builder
            {
                builder.Append("-map 0:0 ");
            }

            foreach (var source in stream.GetSource())
            {
                if (_hasInputBuilder)
                {
                    // If we have an input builder we need to add one to the input file index to account for the input created by our input builder.
                    builder.Append($"-map {_inputFileMap[source] + 1}:{stream.Index} ");
                }
                else
                {
                    builder.Append($"-map {_inputFileMap[source]}:{stream.Index} ");
                }
            }
        }
    }

    private void AppendParameters(StringBuilder builder, ParameterPosition forPosition)
    {
        var parameters = _parameters.Where(x => x.Position == forPosition);
        builder.Append(string.Join(string.Empty, parameters.Select(x => x.Value)));
    }

    /// <summary>
    ///     Create input string for all streams
    /// </summary>
    /// <returns>Input argument</returns>
    private void AppendInputs(StringBuilder builder)
    {
        var index = 0;

        foreach (var source in _streams.SelectMany(x => x.GetSource()).Distinct())
        {
            _inputFileMap[source] = index++;
            builder.Append($"-i {source.Escape()} ");
        }
    }

    private bool HasH264Stream()
    {
        return _streams.Any(stream => stream is IVideoStream { Codec: nameof(VideoCodec.libx264) or nameof(VideoCodec.h264) });
    }

    internal static IConversion Create()
    {
        return new Conversion().SetOverwriteOutput(false);
    }
}
