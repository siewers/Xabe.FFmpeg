namespace Xabe.FFmpeg;

public partial class Conversion
{
    /// <summary>
    ///     Add subtitles to video stream
    /// </summary>
    /// <param name="inputLocation">Video</param>
    /// <param name="outputLocation">Output file</param>
    /// <param name="subtitlesLocation">Subtitles</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> AddSubtitlesAsync(MediaLocation inputLocation, MediaLocation outputLocation, MediaLocation subtitlesLocation)
    {
        var info = await FFmpeg.GetMediaInfo(inputLocation);

        var videoStream = info.VideoStreams.FirstOrDefault()
                              ?.AddSubtitles(subtitlesLocation);

        return Create().AddStream(videoStream)
                       .AddStream(info.AudioStreams.FirstOrDefault())
                       .SetOutput(outputLocation);
    }

    /// <summary>
    ///     Add subtitle to file. It will be added as new stream so if you want to burn subtitles into video you should use
    ///     SetSubtitles method.
    /// </summary>
    /// <param name="inputLocation">Input path</param>
    /// <param name="outputLocation">Output path</param>
    /// <param name="subtitleLocation">Path to subtitle file in .srt format</param>
    /// <param name="language">Language code in ISO 639. Example: "eng", "pol", "pl", "de", "ger"</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> AddSubtitleAsync(MediaLocation inputLocation, MediaLocation outputLocation, MediaLocation subtitleLocation, string? language = null)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(inputLocation);
        var subtitleInfo = await FFmpeg.GetMediaInfo(subtitleLocation);

        var subtitleStream = subtitleInfo.SubtitleStreams.First()
                                         .SetLanguage(language);

        return Create().AddStreams(mediaInfo.VideoStreams)
                       .AddStreams(mediaInfo.AudioStreams)
                       .AddStream(subtitleStream.SetCodec(SubtitleCodec.copy))
                       .SetOutput(outputLocation);
    }

    /// <summary>
    ///     Add subtitle to file. It will be added as new stream so if you want to burn subtitles into video you should use
    ///     SetSubtitles method.
    /// </summary>
    /// <param name="inputLocation">Input path</param>
    /// <param name="outputLocation">Output path</param>
    /// <param name="subtitleLocation">Path to subtitle file in .srt format</param>
    /// <param name="subtitleCodec">The Subtitle Codec to Use to Encode the Subtitles</param>
    /// <param name="language">Language code in ISO 639. Example: "eng", "pol", "pl", "de", "ger"</param>
    /// <returns>Conversion result</returns>
    internal async static Task<IConversion> AddSubtitleAsync(MediaLocation inputLocation, MediaLocation outputLocation, MediaLocation subtitleLocation, SubtitleCodec subtitleCodec, string? language = null)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(inputLocation);
        var subtitleInfo = await FFmpeg.GetMediaInfo(subtitleLocation);

        var subtitleStream = subtitleInfo.SubtitleStreams.First()
                                         .SetLanguage(language);

        return Create().AddStreams(mediaInfo.VideoStreams)
                       .AddStreams(mediaInfo.AudioStreams)
                       .AddStream(subtitleStream.SetCodec(subtitleCodec))
                       .SetOutput(outputLocation);
    }
}
