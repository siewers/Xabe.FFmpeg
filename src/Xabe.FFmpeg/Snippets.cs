namespace Xabe.FFmpeg;

public static class ConversionExtensions
{
    public static Task<IConversionResult> StartConversion(this Task<IConversion> conversionTask, CancellationToken cancellationToken = default)
    {
        return conversionTask.Then(conversion => conversion.Start(cancellationToken));
    }
}

internal static class TaskExtensions
{
    public async static Task<TResult> Then<TSource, TResult>(this Task<TSource> task, Func<TSource, Task<TResult>> next)
    {
        var source = await task;
        return await next(source);
    }
}

public class Snippets
{
    internal Snippets()
    {
    }

    /// <summary>
    ///     Extract audio from file
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output video stream</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ExtractAudio(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.ExtractAudio(inputPath, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Add audio stream to video file
    /// </summary>
    /// <param name="videoPath">Video</param>
    /// <param name="audioPath">Audio</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> AddAudio(string videoPath, string audioPath, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.AddAudio(videoPath, audioPath, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Convert file to MP4
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ToMp4(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.ToMp4(inputPath, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Convert file to TS
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ToTs(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.ToTs(inputPath, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Convert file to OGV
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ToOgv(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.ToOgv(inputPath, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Convert file to WebM
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ToWebM(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.ToWebM(inputPath, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Convert image video stream to gif
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="loop">Number of repeats</param>
    /// <param name="delay">Delay between repeats (in seconds)</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ToGif(string inputPath, string outputPath, int loop, int delay = 0, CancellationToken cancellationToken = default)
    {
        return await Conversion.ToGif(inputPath, outputPath, loop, delay, cancellationToken);
    }

    /// <summary>
    ///     Convert one file to another with destination format using hardware acceleration (if possible). Using cuvid. Works
    ///     only on Windows/Linux with NVidia GPU.
    /// </summary>
    /// <param name="inputFilePath">Path to file</param>
    /// <param name="outputFilePath">Path to file</param>
    /// <param name="hardwareAccelerator">
    ///     Hardware accelerator. List of all acceclerators available for your system - "ffmpeg
    ///     -hwaccels"
    /// </param>
    /// <param name="decoder">Codec using to decoding input video (e.g. h264_cuvid)</param>
    /// <param name="encoder">Codec using to encode output video (e.g. h264_nvenc)</param>
    /// <param name="device">Number of device (0 = default video card) if more than one video card.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>IConversion object</returns>
    public async Task<IConversion> ConvertWithHardware(string inputFilePath, string outputFilePath, HardwareAccelerator hardwareAccelerator, VideoCodec decoder, VideoCodec encoder, int device = 0, CancellationToken cancellationToken = default)
    {
        return await Conversion.ConvertWithHardwareAsync(inputFilePath, outputFilePath, hardwareAccelerator, decoder, encoder, device);
    }

    /// <summary>
    ///     Add subtitles to video stream
    /// </summary>
    /// <param name="inputPath">Video</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="subtitlesPath">Subtitles</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> BurnSubtitle(string inputPath, string outputPath, string subtitlesPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.AddSubtitlesAsync(inputPath, outputPath, subtitlesPath);
    }

    /// <summary>
    ///     Add subtitle to file. It will be added as new stream so if you want to burn subtitles into video you should use
    ///     BurnSubtitle method.
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="subtitlePath">Path to subtitle file in .srt format</param>
    /// <param name="language">Language code in ISO 639. Example: "eng", "pol", "pl", "de", "ger"</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> AddSubtitle(string inputPath, string outputPath, string subtitlePath, string? language = null, CancellationToken cancellationToken = default)
    {
        return await Conversion.AddSubtitleAsync(inputPath, outputPath, subtitlePath, language);
    }

    /// <summary>
    ///     Add subtitle to file. It will be added as new stream so if you want to burn subtitles into video you should use
    ///     BurnSubtitle method.
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="subtitlePath">Path to subtitle file in .srt format</param>
    /// <param name="subtitleCodec">The Subtitle Codec to Use to Encode the Subtitles</param>
    /// <param name="language">Language code in ISO 639. Example: "eng", "pol", "pl", "de", "ger"</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> AddSubtitle(string inputPath, string outputPath, string subtitlePath, SubtitleCodec subtitleCodec, string? language = null, CancellationToken cancellationToken = default)
    {
        return await Conversion.AddSubtitleAsync(inputPath, outputPath, subtitlePath, subtitleCodec, language);
    }

    /// <summary>
    ///     Melt watermark into video
    /// </summary>
    /// <param name="inputPath">Input video path</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="inputImage">Watermark</param>
    /// <param name="position">Position of watermark</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> SetWatermark(string inputPath, string outputPath, string inputImage, Position position, CancellationToken cancellationToken = default)
    {
        return await Conversion.SetWatermarkAsync(inputPath, outputPath, inputImage, position, cancellationToken);
    }

    /// <summary>
    ///     Extract video from file
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output audio stream</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ExtractVideo(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        return await Conversion.ExtractVideoAsync(inputPath, outputPath, cancellationToken);
    }

    /// <summary>
    ///     Saves snapshot of video
    /// </summary>
    /// <param name="inputPath">Video</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="captureTime">TimeSpan of snapshot</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> Snapshot(string inputPath, string outputPath, TimeSpan captureTime, CancellationToken cancellationToken = default)
    {
        return await Conversion.SnapshotAsync(inputPath, outputPath, captureTime, cancellationToken);
    }

    /// <summary>
    ///     Change video size
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="width">Expected width</param>
    /// <param name="height">Expected height</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ChangeSize(string inputPath, string outputPath, int width, int height, CancellationToken cancellationToken = default)
    {
        return await Conversion.ChangeSizeAsync(inputPath, outputPath, width, height, cancellationToken);
    }

    /// <summary>
    ///     Change video size
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="size">Expected size</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> ChangeSize(string inputPath, string outputPath, VideoSize size, CancellationToken cancellationToken = default)
    {
        return await Conversion.ChangeSizeAsync(inputPath, outputPath, size, cancellationToken);
    }

    /// <summary>
    ///     Get part of video
    /// </summary>
    /// <param name="inputPath">Video</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="startTime">Start point</param>
    /// <param name="duration">Duration of new video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> Split(string inputPath, string outputPath, TimeSpan startTime, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        return await Conversion.SplitAsync(inputPath, outputPath, startTime, duration, cancellationToken);
    }

    /// <summary>
    ///     Save M3U8 stream
    /// </summary>
    /// <param name="uri">Uri to stream</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="duration">Duration of stream</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> SaveM3U8Stream(Uri uri, string outputPath, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        return await Conversion.SaveM3U8StreamAsync(uri, outputPath, duration, cancellationToken);
    }

    /// <summary>
    ///     Concat multiple inputVideos.
    /// </summary>
    /// <param name="output">Concatenated inputVideos</param>
    /// <param name="inputVideos">Videos to add</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    public async Task<IConversion> Concatenate(string output, string[] inputVideos, CancellationToken cancellationToken = default)
    {
        return await Conversion.Concatenate(output, inputVideos, cancellationToken);
    }

    /// <summary>
    ///     Convert one file to another with destination format.
    /// </summary>
    /// <param name="inputFilePath">Path to file</param>
    /// <param name="outputFilePath">Path to file</param>
    /// <param name="keepSubtitles">Whether to Keep Subtitles in the output video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>IConversion object</returns>
    public async Task<IConversion> Convert(string inputFilePath, string outputFilePath, bool keepSubtitles = false, CancellationToken cancellationToken = default)
    {
        return await Conversion.ConvertAsync(inputFilePath, outputFilePath, keepSubtitles, cancellationToken);
    }

    /// <summary>
    ///     Transcode one file to another with destination format and codecs.
    /// </summary>
    /// <param name="inputFilePath">Path to file</param>
    /// <param name="outputFilePath">Path to file</param>
    /// <param name="audioCodec"> The audio codec to Transcode the input to</param>
    /// <param name="videoCodec"> The video codec to Transcode the input to</param>
    /// <param name="subtitleCodec"> The subtitle codec to Transcode the input to</param>
    /// <param name="keepSubtitles">Whether to keep subtitles in the output video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>IConversion object</returns>
    public async Task<IConversion> Transcode(string inputFilePath, string outputFilePath, VideoCodec videoCodec, AudioCodec audioCodec, SubtitleCodec subtitleCodec, bool keepSubtitles = false, CancellationToken cancellationToken = default)
    {
        return await Conversion.TranscodeAsync(inputFilePath, outputFilePath, videoCodec, audioCodec, subtitleCodec, keepSubtitles, cancellationToken);
    }

    /// <summary>
    ///     Generates a visualization of an audio stream using the 'showfreqs' filter
    /// </summary>
    /// <param name="inputPath">Path to the input file containing the audio stream to visualise</param>
    /// <param name="outputPath">Path to output the visualised audio stream to</param>
    /// <param name="size">The Size of the outputted video stream</param>
    /// <param name="pixelFormat">The output pixel format (default is yuv420p)</param>
    /// <param name="mode">The visualisation mode (default is bar)</param>
    /// <param name="amplitudeScale">The frequency scale (default is lin)</param>
    /// <param name="frequencyScale">The amplitude scale (default is log)</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>IConversion object</returns>
    public async Task<IConversion> VisualizeAudio
    (
        string inputPath, string outputPath, VideoSize size,
        PixelFormat pixelFormat = PixelFormat.yuv420p,
        VisualisationMode mode = VisualisationMode.bar,
        AmplitudeScale amplitudeScale = AmplitudeScale.lin,
        FrequencyScale frequencyScale = FrequencyScale.log,
        CancellationToken cancellationToken = default
    )
    {
        return await Conversion.VisualizeAudio(inputPath, outputPath, size, pixelFormat, mode, amplitudeScale, frequencyScale, cancellationToken);
    }

    /// <summary>
    ///     Loop file infinitely to rtsp streams with some default parameters like: -re, -preset ultrafast
    /// </summary>
    /// <param name="inputFilePath">Path to file</param>
    /// <param name="rtspServerUri">Uri of RTSP Server in format: rtsp://127.0.0.1:8554/name</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>IConversion object</returns>
    public async Task<IConversion> SendToRtspServer(string inputFilePath, Uri rtspServerUri, CancellationToken cancellationToken = default)
    {
        return await Conversion.SendToRtspServer(inputFilePath, rtspServerUri, cancellationToken);
    }

    /// <summary>
    ///     Send desktop infinitely to rtsp streams with some default parameters like: -re, -preset ultrafast
    /// </summary>
    /// <param name="rtspServerUri">Uri of RTSP Server in format: rtsp://127.0.0.1:8554/name</param>
    /// <returns>IConversion object</returns>
    public IConversion SendDesktopToRtspServer(Uri rtspServerUri)
    {
        return Conversion.SendDesktopToRtspServer(rtspServerUri);
    }
}
