namespace Xabe.FFmpeg.Probe;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Exceptions;
using Models;

/// <inheritdoc cref="Xabe.FFmpeg.Probe.IMediaInfo" />
[PublicAPI]
public sealed class MediaInfo : IMediaInfo
{
    private MediaInfo(FileInfo mediaFile, ProbeModel probeModel)
    {
        Path = mediaFile;
        Size = probeModel.Format.Size;
        CreationTime = probeModel.Format.Tags.CreationTime?.UtcDateTime;
        VideoStreams = probeModel.Streams.OfType<VideoStreamModel>().Select(stream => new VideoStream(stream, probeModel.Format));
        AudioStreams = probeModel.Streams.OfType<AudioStreamModel>().Select(stream => new AudioStream(stream, probeModel.Format));
        SubtitleStreams = probeModel.Streams.OfType<SubtitleStreamModel>().Select(stream => new SubtitleStream(stream, probeModel.Format));
        Duration = CalculateDuration(probeModel);
    }

    /// <inheritdoc />
    public FileInfo Path { get; }

    /// <inheritdoc />
    public DateTime? CreationTime { get; internal set; }

    /// <inheritdoc />
    public long Size { get; internal set; }

    /// <inheritdoc />
    public TimeSpan Duration { get; internal set; }

    /// <inheritdoc />
    public IEnumerable<IStream> Streams => [..VideoStreams, ..AudioStreams, ..SubtitleStreams];

    /// <inheritdoc />
    public IEnumerable<IVideoStream> VideoStreams { get; internal set; }

    /// <inheritdoc />
    public IEnumerable<IAudioStream> AudioStreams { get; internal set; }

    /// <inheritdoc />
    public IEnumerable<ISubtitleStream> SubtitleStreams { get; internal set; }

    /// <summary>
    ///     Get MediaInfo from file
    /// </summary>
    /// <param name="mediaFile">The media file to get information from</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async static Task<IMediaInfo> Get(FileInfo mediaFile, CancellationToken cancellationToken = default)
    {
        if (!mediaFile.Exists)
        {
            throw new InvalidInputException($"Input file {mediaFile.FullName} doesn't exists.");
        }

        using var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);

        var wrapper = new FFprobeWrapper();
        var probeResult = await wrapper.GetProbeModel(mediaFile, cancellationTokenSource.Token);
        return new MediaInfo(mediaFile, probeResult);
    }

    private static TimeSpan CalculateDuration(ProbeModel probeModel)
    {
        var audioMax = probeModel.Streams.OfType<AudioStreamModel>().Max(stream => stream.Duration);
        var videoMax = probeModel.Streams.OfType<VideoStreamModel>().Max(stream => stream.Duration);

        return (audioMax > videoMax ? audioMax : videoMax) ?? probeModel.Format.Duration;
    }
}
