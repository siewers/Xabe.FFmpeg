namespace Xabe.FFmpeg;

public partial class Conversion
{
    /// <summary>
    ///     Extract audio from file
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Output video stream</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> ExtractAudio(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var audioStream = info.AudioStreams.FirstOrDefault();

        if (audioStream is null)
        {
            throw new InvalidOperationException($"No audio stream found in {inputPath}");
        }

        return Create()
               .AddStream(audioStream)
               .SetAudioBitrate(audioStream.Bitrate)
               .SetOutput(outputPath);
    }

    /// <summary>
    ///     Add audio stream to video file
    /// </summary>
    /// <param name="videoPath">Video</param>
    /// <param name="audioPath">Audio</param>
    /// <param name="outputPath">Output file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> AddAudio(string videoPath, string audioPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var videoInfo = await FFmpeg.GetMediaInfo(videoPath, cancellationToken);
        var audioInfo = await FFmpeg.GetMediaInfo(audioPath, cancellationToken);

        return Create()
               .AddStreams(videoInfo.VideoStreams)
               .AddStreams(videoInfo.SubtitleStreams)
               .AddStreams(audioInfo.AudioStreams)
               .SetOutput(outputPath);
    }

    /// <summary>
    ///     Generates a visualisation of an audio stream using the 'showfreqs' filter
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
    internal async static Task<IConversion> VisualizeAudio
    (
        string inputPath, string outputPath, VideoSize size,
        PixelFormat pixelFormat = PixelFormat.yuv420p,
        VisualisationMode mode = VisualisationMode.bar,
        AmplitudeScale amplitudeScale = AmplitudeScale.lin,
        FrequencyScale frequencyScale = FrequencyScale.log,
        CancellationToken cancellationToken = default
    )
    {
        var inputInfo = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);
        var audioStream = inputInfo.AudioStreams.FirstOrDefault();
        var videoStream = inputInfo.VideoStreams.FirstOrDefault();

        var filter = $"\"[0:a]showfreqs=mode={mode.ToStringFast()}:fscale={frequencyScale.ToStringFast()}:ascale={amplitudeScale.ToStringFast()},format={pixelFormat.ToStringFast()},scale={size.ToFFmpegFormat()} [v]\"";

        return Create()
               .AddStream(audioStream)
               .AddParameter($"-filter_complex {filter}")
               .AddParameter("-map [v]")
               .SetFramerate(videoStream?.Framerate ?? 30) // Pin frame rate at the original rate or 30 fps to stop dropped or duplicated frames
               .SetOutput(outputPath);
    }
}
