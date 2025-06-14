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
    private Func<string, string>? _buildInputFileName;
    private Func<string, string>? _buildOutputFileName;
    private FFmpegWrapper? _ffmpeg;
    private bool _hasInputBuilder;
    private string? _output;
    private ProcessPriorityClass? _priority;

    public Conversion()
    {
        _userDefinedParameters[key: ParameterPosition.PostInput] = [];
        _userDefinedParameters[key: ParameterPosition.PreInput] = [];
    }

    /// <inheritdoc />
    public string Build()
    {
        lock (_builderLock)
        {
            var builder = new StringBuilder();

            _buildOutputFileName ??= _ => _output;

            builder.Append(value: string.Join(" ", _userDefinedParameters[key: ParameterPosition.PreInput].Select(selector: x => x.Trim())) + " ");
            builder.Append(value: GetParameters(forPosition: ParameterPosition.PreInput));
            builder.Append(value: GetStreamsPreInputs());

            if (_buildInputFileName is not null)
            {
                _hasInputBuilder = true;
                builder.Append(value: _buildInputFileName(arg: "_%03d"));
            }

            builder.Append(value: GetInputs());

            builder.Append(value: GetStreamsPostInputs());
            builder.Append(value: GetFilters());
            builder.Append(value: GetMap());
            builder.Append(value: GetParameters(forPosition: ParameterPosition.PostInput));
            builder.Append(value: string.Join(" ", _userDefinedParameters[key: ParameterPosition.PostInput].Select(selector: x => x.Trim())) + " ");
            builder.Append(value: _buildOutputFileName(arg: "_%03d"));

            return builder.ToString();
        }
    }

    /// <inheritdoc />
    public event ConversionProgressEventHandler? OnProgress;

    /// <inheritdoc />
    public event DataReceivedEventHandler? OnDataReceived;

    /// <inheritdoc />
    public event VideoDataEventHandler? OnVideoDataReceived;

    /// <inheritdoc />
    public string OutputFilePath { get; private set; }

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
    public Task<IConversionResult> Start(string parameters)
    {
        return Start(parameters, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<IConversionResult> Start(string parameters, CancellationToken cancellationToken)
    {
        if (_ffmpeg is not null)
        {
            throw new InvalidOperationException(message: "Conversion has already been started.");
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
                       StartTime = new DateTime(ticks: startTime),
                       EndTime = new DateTime(ticks: endTime),
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
        _userDefinedParameters[key: parameterPosition].Add(item: parameter);
        return this;
    }

    /// <inheritdoc />
    public IConversion AddStream<T>(T? stream) where T : IStream
    {
        if (stream is not null)
        {
            _streams.Add(item: stream);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion AddStreams(IEnumerable<IStream?> streams)
    {
        foreach (var stream in streams)
        {
            AddStream(stream: stream);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetHashFormat(Hash hashFormat = Hash.SHA256)
    {
        var format = hashFormat switch
        {
            Hash.SHA512_256 => "SHA512/256",
            Hash.SHA512_224 => "SHA512/224",
            _ => hashFormat.ToStringFast(),
        };

        SetOutputFormat(outputFormat: Format.hash);
        return SetHashFormat(hashFormat: format);
    }

    /// <inheritdoc />
    public IConversion SetHashFormat(string hashFormat)
    {
        _parameters.Add("hash", hashFormat, ParameterPosition.PostInput);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetPreset(ConversionPreset preset)
    {
        _parameters.Add("preset", preset.ToStringFast().ToLower(), ParameterPosition.PostInput);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetSeek(TimeSpan? seek)
    {
        if (seek.HasValue)
        {
            _parameters.Add("ss", seek.Value, ParameterPosition.PostInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetInputTime(TimeSpan? time)
    {
        if (time.HasValue)
        {
            _parameters.Add("t", time.Value, ParameterPosition.PreInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetOutputTime(TimeSpan? time)
    {
        if (time.HasValue)
        {
            _parameters.Add("t", time.Value, ParameterPosition.PostInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion UseMultiThread(bool multiThread)
    {
        var threads = multiThread ? Environment.ProcessorCount : 1;
        _parameters.Add("threads", Math.Min(threads, 16));
        return this;
    }

    /// <inheritdoc />
    public IConversion UseMultiThread(int threadsCount)
    {
        _parameters.Add("threads", threadsCount);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetOutput(string outputFilePath)
    {
        OutputFilePath = new FileInfo(fileName: outputFilePath).FullName;
        _output = outputFilePath.Escape();
        return this;
    }

    /// <inheritdoc />
    public IConversion PipeOutput(PipeDescriptor descriptor = PipeDescriptor.stdout)
    {
        SetOutput(outputFilePath: $"pipe:{descriptor.ToStringFast()}");
        OutputPipeDescriptor = descriptor;
        return this;
    }

    /// <inheritdoc />
    public IConversion SetVideoBitrate(long bitrate)
    {
        _parameters.Add("b:v", bitrate, ParameterPosition.PostInput);
        _parameters.Add("minrate", bitrate, ParameterPosition.PostInput);
        _parameters.Add("maxrate", bitrate, ParameterPosition.PostInput);
        _parameters.Add("bufsize", bitrate, ParameterPosition.PostInput);

        if (HasH264Stream())
        {
            _parameters.Add("x264opts", "nal-hrd=cbr:force-cfr=1", ParameterPosition.PostInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetAudioBitrate(long bitrate)
    {
        _parameters.Add("b:a", bitrate, ParameterPosition.PostInput);
        return this;
    }

    /// <inheritdoc />
    public IConversion UseShortest(bool useShortest)
    {
        if (useShortest)
        {
            _parameters.Add("shortest", ParameterPosition.PostInput);
        }
        else
        {
            _parameters.Remove("shortest", ParameterPosition.PostInput);
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
    public IConversion ExtractEveryNthFrame(int frameNo, Func<string, string> buildOutputFileName)
    {
        _buildOutputFileName = buildOutputFileName;
        OutputFilePath = buildOutputFileName(arg: "");
        _parameters.Add("vf", $"select='not(mod(n\\,{frameNo}))'", ParameterPosition.PostInput);
        SetVideoSyncMethod(method: VideoSyncMethod.vfr);

        return this;
    }

    /// <inheritdoc />
    public IConversion ExtractNthFrame(int frameNo, Func<string, string> buildOutputFileName)
    {
        _buildOutputFileName = buildOutputFileName;
        _parameters.Add("vf", $"select='eq(n\\,{frameNo})'", ParameterPosition.PostInput);
        OutputFilePath = buildOutputFileName(arg: "");
        SetVideoSyncMethod(method: VideoSyncMethod.passthrough);
        return this;
    }

    /// <inheritdoc />
    public IConversion BuildVideoFromImages(int startNumber, Func<string, string> buildInputFileName)
    {
        _buildInputFileName = buildInputFileName;
        _parameters.Add("start_number", startNumber, ParameterPosition.PreInput);
        return this;
    }

    /// <inheritdoc />
    public IConversion BuildVideoFromImages(IEnumerable<string> imageFiles)
    {
        var builder = new InputBuilder();
        _buildInputFileName = builder.PrepareInputFiles(imageFiles.ToList(), out _);

        return this;
    }

    /// <inheritdoc />
    public IConversion SetInputFramerate(double framerate)
    {
        _parameters.Add("framerate", framerate.ToFFmpegFormat(decimalPlaces: 3), ParameterPosition.PreInput);
        _parameters.Add("r", framerate.ToFFmpegFormat(decimalPlaces: 3), ParameterPosition.PreInput);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetFramerate(double framerate)
    {
        _parameters.Add("framerate", framerate.ToFFmpegFormat(decimalPlaces: 3), ParameterPosition.PostInput);
        _parameters.Add("r", framerate.ToFFmpegFormat(decimalPlaces: 3), ParameterPosition.PostInput);
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
        _parameters.Add("hwaccel", hardwareAccelerator, ParameterPosition.PreInput);
        _parameters.Add("c:v", decoder, ParameterPosition.PreInput);

        _parameters.Add("c:v", encoder, ParameterPosition.PostInput);

        if (device != 0)
        {
            _parameters.Add("hwaccel_device", device, ParameterPosition.PreInput);
        }

        UseMultiThread(multiThread: false);
        return this;
    }

    /// <inheritdoc />
    public IConversion SetOverwriteOutput(bool overwrite)
    {
        if (overwrite)
        {
            _parameters.Add("y", ParameterPosition.PostInput);
            _parameters.Remove("n", ParameterPosition.PostInput);
        }
        else
        {
            _parameters.Add("n", ParameterPosition.PostInput);
            _parameters.Remove("y", ParameterPosition.PostInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetInputFormat(Format inputFormat)
    {
        var format = inputFormat.ToStringFast(useMetadataAttributes: true);
        return SetInputFormat(format: format);
    }

    /// <inheritdoc />
    public IConversion SetInputFormat(string? format)
    {
        if (format is not null)
        {
            _parameters.Add("f", format, ParameterPosition.PreInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetOutputFormat(Format outputFormat)
    {
        var format = outputFormat.ToStringFast(useMetadataAttributes: true);
        return SetOutputFormat(format: format);
    }

    /// <inheritdoc />
    public IConversion SetOutputFormat(string? format)
    {
        if (format is not null)
        {
            _parameters.Add("f", format, ParameterPosition.PostInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetPixelFormat(PixelFormat pixelFormat)
    {
        var format = pixelFormat.ToStringFast(useMetadataAttributes: true);

        return SetPixelFormat(pixelFormat: format);
    }

    /// <inheritdoc />
    public IConversion SetPixelFormat(string? pixelFormat)
    {
        if (pixelFormat is not null)
        {
            _parameters.Add("pix_fmt", pixelFormat, ParameterPosition.PostInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion SetVideoSyncMethod(VideoSyncMethod method)
    {
        if (method == VideoSyncMethod.auto)
        {
            _parameters.Add("vsync", -1, ParameterPosition.PostInput);
        }
        else
        {
            _parameters.Add("vsync", method.ToStringFast(), ParameterPosition.PostInput);
        }

        return this;
    }

    /// <inheritdoc />
    public IConversion AddDesktopStream(string? videoSize = null, double framerate = 30, int xOffset = 0, int yOffset = 0)
    {
        var (path, format) = GetPathAndFormat();
        var index = _streams.Count != 0 ? _streams.Max(selector: x => x.Index) + 1 : 0;

        var stream = new VideoStream(path, index);

        stream.SetInputFormat(inputFormat: format);

        stream.Parameters.Add("framerate", framerate.ToFFmpegFormat(decimalPlaces: 4), ParameterPosition.PreInput);
        stream.Parameters.Add("offset_x", xOffset, ParameterPosition.PreInput);
        stream.Parameters.Add("offset_y", yOffset, ParameterPosition.PreInput);

        if (videoSize is not null)
        {
            stream.Parameters.Add("video_size", videoSize, ParameterPosition.PreInput);
        }

        AddStream(stream: stream);

        return this;

        (string Path, Format Format) GetPathAndFormat()
        {
            if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows))
            {
                return (Path: "desktop", Format: Format.gdigrab);
            }

            if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.OSX))
            {
                return (Path: "1:1", Format: Format.avfoundation);
            }

            if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Linux))
            {
                return (Path: ":0.0+0,0", Format: Format.x11grab);
            }

            throw new PlatformNotSupportedException();
        }
    }

    /// <inheritdoc />
    public IConversion AddDesktopStream(VideoSize videoSize, double framerate = 30, int xOffset = 0, int yOffset = 0)
    {
        return AddDesktopStream(videoSize.ToFFmpegFormat(), framerate, xOffset, yOffset);
    }

    private void CreateOutputDirectoryIfNotExists()
    {
        if (string.IsNullOrWhiteSpace(value: OutputFilePath) || OutputPipeDescriptor is not null)
        {
            return;
        }

        try
        {
            if (!Directory.Exists(path: Path.GetDirectoryName(path: OutputFilePath.Unescape())))
            {
                Directory.CreateDirectory(path: Path.GetDirectoryName(path: OutputFilePath.Unescape()));
            }
        }
        catch (IOException)
        {
        }
    }

    private string GetStreamsPostInputs()
    {
        var builder = new StringBuilder();

        foreach (var stream in _streams)
        {
            builder.Append(value: stream.BuildParameters(forPosition: ParameterPosition.PostInput));
        }

        return builder.ToString();
    }

    private string GetStreamsPreInputs()
    {
        var builder = new StringBuilder();

        foreach (var stream in _streams)
        {
            builder.Append(value: stream.BuildParameters(forPosition: ParameterPosition.PreInput));
        }

        return builder.ToString();
    }

    private string GetFilters()
    {
        var builder = new StringBuilder();
        var configurations = new List<IFilterConfiguration>();

        configurations.AddRange(collection: _streams.OfType<IFilterable>().SelectMany(selector: filterable => filterable.GetFilters()));

        var filterGroups = configurations.GroupBy(keySelector: configuration => configuration.FilterType);

        foreach (var filterGroup in filterGroups)
        {
            builder.Append(handler: $"{filterGroup.Key} \"");

            foreach (var configuration in configurations.Where(predicate: x => x.FilterType == filterGroup.Key))
            {
                var values = new List<string>();

                foreach (var filter in configuration.Filters)
                {
                    var map = $"[{configuration.StreamNumber}]";
                    var value = string.IsNullOrEmpty(value: filter.Value) ? $"{filter.Key} " : $"{filter.Key}={filter.Value}";
                    values.Add(item: $"{map} {value} ");
                }

                builder.Append(value: string.Join(";", values));
            }

            builder.Append(value: "\" ");
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Create map for included streams, including the InputBuilder if required
    /// </summary>
    /// <returns>Map argument</returns>
    private string GetMap()
    {
        var builder = new StringBuilder();

        foreach (var stream in _streams)
        {
            if (_hasInputBuilder) // If we have an input builder we always want to map the first video stream as it will be created by our input builder
            {
                builder.Append(value: "-map 0:0 ");
            }

            foreach (var source in stream.GetSource())
            {
                if (_hasInputBuilder)
                {
                    // If we have an input builder we need to add one to the input file index to account for the input created by our input builder.
                    builder.Append(handler: $"-map {_inputFileMap[key: source] + 1}:{stream.Index} ");
                }
                else
                {
                    builder.Append(handler: $"-map {_inputFileMap[key: source]}:{stream.Index} ");
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Create parameters string
    /// </summary>
    /// <param name="forPosition">Position for parameters</param>
    /// <returns>Parameters</returns>
    private string GetParameters(ParameterPosition forPosition)
    {
        var parameters = _parameters.Where(predicate: x => x.Position == forPosition);
        return string.Join(string.Empty, parameters.Select(selector: x => x.Parameter));
    }

    /// <summary>
    ///     Create input string for all streams
    /// </summary>
    /// <returns>Input argument</returns>
    private string GetInputs()
    {
        var builder = new StringBuilder();
        var index = 0;

        foreach (var source in _streams.SelectMany(selector: x => x.GetSource()).Distinct())
        {
            _inputFileMap[key: source] = index++;
            builder.Append(handler: $"-i {source.Escape()} ");
        }

        return builder.ToString();
    }

    private bool HasH264Stream()
    {
        return _streams.Any(predicate: stream => stream is IVideoStream { Codec: nameof(VideoCodec.libx264) or nameof(VideoCodec.h264) });
    }

    internal static IConversion Create()
    {
        return new Conversion().SetOverwriteOutput(overwrite: false);
    }
}
