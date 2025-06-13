namespace Xabe.FFmpeg;

using Probe;

/// <inheritdoc />
public partial class Conversion
{
    /// <summary>
    ///     Melt watermark into video
    /// </summary>
    /// <param name="inputPath">Input video path</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="inputImage">Watermark</param>
    /// <param name="position">Position of watermark</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> SetWatermarkAsync(string inputPath, string outputPath, string inputImage, Position position, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var videoStream = info.VideoStreams.FirstOrDefault()?.SetWatermark(inputImage, position);

        return Create().AddStream(videoStream)
                       .AddStreams(info.AudioStreams.ToArray())
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Extract video from file
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output audio stream</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> ExtractVideoAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var videoStream = info.VideoStreams.FirstOrDefault();

        return Create().AddStream(videoStream)
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Saves snapshot of video
    /// </summary>
    /// <param name="inputPath">Video</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="captureTime">TimeSpan of snapshot</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> SnapshotAsync(string inputPath, string outputPath, TimeSpan captureTime, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var videoStream = info.VideoStreams.FirstOrDefault()?.SetOutputFramesCount(1).SetSeek(captureTime);

        return Create().AddStream(videoStream)
                       .SetOutput(outputPath);
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
    internal async static Task<IConversion> ChangeSizeAsync(string inputPath, string outputPath, int width, int height, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var videoStream = info.VideoStreams.FirstOrDefault()?.SetSize(width, height);

        return Create().AddStream(videoStream)
                       .AddStreams(info.AudioStreams.ToArray())
                       .AddStreams(info.SubtitleStreams.ToArray())
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Change video size
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="size">Expected size</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> ChangeSizeAsync(string inputPath, string outputPath, VideoSize size, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var videoStream = info.VideoStreams.FirstOrDefault()?.SetSize(size);

        return Create().AddStream(videoStream)
                       .AddStreams(info.AudioStreams.ToArray())
                       .AddStreams(info.SubtitleStreams.ToArray())
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Get part of video
    /// </summary>
    /// <param name="inputPath">The path to the input file</param>
    /// <param name="outputPath">The path to the output file</param>
    /// <param name="startTime">The start time</param>
    /// <param name="duration">Duration of new video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> SplitAsync(string inputPath, string outputPath, TimeSpan startTime, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var streams = info.VideoStreams.Select(stream => stream.Split(startTime, duration)).Cast<IStream>().ToList();
        streams.AddRange(info.AudioStreams.Select(stream => stream.Split(startTime, duration)));

        return Create().AddStreams(streams)
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Save M3U8 stream
    /// </summary>
    /// <param name="uri">Uri to stream</param>
    /// <param name="outputPath">Output path</param>
    /// <param name="duration">Duration of stream</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> SaveM3U8StreamAsync(Uri uri, string outputPath, TimeSpan? duration = null, CancellationToken cancellationToken = default)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(uri.ToString(), cancellationToken);
        return Create().AddStreams(mediaInfo.Streams)
                       .SetInputTime(duration)
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Concat multiple inputVideos.
    /// </summary>
    /// <param name="output">Concatenated inputVideos</param>
    /// <param name="inputVideos">Videos to add</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> Concatenate(string output, string[] inputVideos, CancellationToken cancellationToken = default)
    {
        if (inputVideos.Length <= 1)
        {
            throw new ArgumentException("You must provide at least 2 files for the concatenation to work", nameof(inputVideos));
        }

        var mediaInfos = new List<IMediaInfo>();

        var conversion = Create();

        foreach (var inputVideo in inputVideos)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(inputVideo, cancellationToken);

            mediaInfos.Add(mediaInfo);
            conversion.AddParameter($"-i {inputVideo.Escape()} ");
        }

        conversion.AddParameter("-t 1 -f lavfi -i anullsrc=r=48000:cl=stereo");
        conversion.AddParameter("-filter_complex \"");

        var maxResolutionMedia = mediaInfos.Select(x => x.VideoStreams.OrderByDescending(z => z.Width).First())
                                           .OrderByDescending(x => x.Width)
                                           .First();

        for (var i = 0; i < mediaInfos.Count; i++)
        {
            conversion.AddParameter($"[{i}:v]scale={maxResolutionMedia.Width}:{maxResolutionMedia.Height},setdar=dar={maxResolutionMedia.Ratio},setpts=PTS-STARTPTS[v{i}]; ");
        }

        for (var i = 0; i < mediaInfos.Count; i++)
        {
            conversion.AddParameter(!mediaInfos[i].AudioStreams.Any() ? $"[v{i}]" : $"[v{i}][{i}:a]");
        }

        conversion.AddParameter($"concat=n={inputVideos.Length}:v=1:a=1 [v] [a]\" -map \"[v]\" -map \"[a]\"");
        conversion.AddParameter($"-aspect {maxResolutionMedia.Ratio}");
        return conversion.SetOutput(output);
    }

    /// <summary>
    ///     Convert one file to another with destination format.
    /// </summary>
    /// <param name="inputFilePath">Path to file</param>
    /// <param name="outputFilePath">Path to file</param>
    /// <param name="keepSubtitles">Whether to Keep Subtitles in the output video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>IConversion object</returns>
    internal async static Task<IConversion> ConvertAsync(string inputFilePath, string outputFilePath, bool keepSubtitles = false, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputFilePath);

        var conversion = Create().SetOutput(outputFilePath);

        foreach (var stream in info.Streams)
        {
            switch (stream)
            {
                case IVideoStream videoStream:
                    // PR #268 We have to force the frame rate here due to an FFmpeg bug with videos > 100fps from android devices
                    conversion.AddStream(videoStream.SetFramerate(videoStream.Framerate));
                    break;
                case IAudioStream audioStream:
                    conversion.AddStream(audioStream);
                    break;
                case ISubtitleStream subtitleStream when keepSubtitles:
                    conversion.AddStream(subtitleStream.SetCodec(SubtitleCodec.mov_text));
                    break;
            }
        }

        return conversion;
    }

    /// <summary>
    ///     Transcode one file to another with destination format and codecs.
    /// </summary>
    /// <param name="inputFilePath">Path to the input file</param>
    /// <param name="outputFilePath">Path to the output file</param>
    /// <param name="audioCodec">The audio codec to transcode the input to</param>
    /// <param name="videoCodec">The video codec to transcode the input to</param>
    /// <param name="subtitleCodec">The subtitle codec to transcode the input to</param>
    /// <param name="keepSubtitles">Whether to keep subtitles in the output video</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>IConversion object</returns>
    internal async static Task<IConversion> TranscodeAsync(string inputFilePath, string outputFilePath, VideoCodec videoCodec, AudioCodec audioCodec, SubtitleCodec subtitleCodec, bool keepSubtitles = false, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputFilePath, cancellationToken);

        var conversion = Create().SetOutput(outputFilePath);

        foreach (var stream in info.Streams)
        {
            switch (stream)
            {
                case IVideoStream videoStream:
                    // PR #268 We have to force the framerate here due to an FFmpeg bug with videos > 100fps from android devices
                    conversion.AddStream(videoStream.SetCodec(videoCodec).SetFramerate(videoStream.Framerate));
                    break;
                case IAudioStream audioStream:
                    conversion.AddStream(audioStream.SetCodec(audioCodec));
                    break;
                case ISubtitleStream subtitleStream when keepSubtitles:
                    conversion.AddStream(subtitleStream.SetCodec(subtitleCodec));
                    break;
            }
        }

        return conversion;
    }
}
