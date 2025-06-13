namespace Xabe.FFmpeg;

public partial class Conversion
{
    /// <summary>
    ///     Convert file to MP4
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> ToMp4(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        IStream? videoStream = info.VideoStreams.FirstOrDefault()?.SetCodec(VideoCodec.h264);
        IStream? audioStream = info.AudioStreams.FirstOrDefault()?.SetCodec(AudioCodec.aac);

        return Create().AddStreams([videoStream, audioStream])
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Convert file to TS
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> ToTs(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        IStream? videoStream = info.VideoStreams.FirstOrDefault()?.SetCodec(VideoCodec.mpeg2video);
        IStream? audioStream = info.AudioStreams.FirstOrDefault()?.SetCodec(AudioCodec.mp2);

        return Create().AddStreams([videoStream, audioStream])
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Convert file to OGV
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> ToOgv(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        IStream? videoStream = info.VideoStreams.FirstOrDefault()?.SetCodec(VideoCodec.theora);
        IStream? audioStream = info.AudioStreams.FirstOrDefault()?.SetCodec(AudioCodec.libvorbis);

        return Create().AddStreams([videoStream, audioStream])
                       .SetOutput(outputPath);
    }

    /// <summary>
    ///     Convert file to WebM
    /// </summary>
    /// <param name="inputPath">Input path</param>
    /// <param name="outputPath">Destination file</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> ToWebM(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        IStream? videoStream = info.VideoStreams.FirstOrDefault()?.SetCodec(VideoCodec.vp8);
        IStream? audioStream = info.AudioStreams.FirstOrDefault()?.SetCodec(AudioCodec.libvorbis);

        return Create().AddStreams([videoStream, audioStream])
                       .SetOutput(outputPath);
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
    internal async static Task<IConversion> ToGif(string inputPath, string outputPath, int loop, int delay = 0, CancellationToken cancellationToken = default)
    {
        var info = await FFmpeg.GetMediaInfo(inputPath, cancellationToken);

        var videoStream = info.VideoStreams.FirstOrDefault()?.SetLoop(loop, delay);

        return Create().AddStream(videoStream)
                       .SetOutput(outputPath);
    }
}
