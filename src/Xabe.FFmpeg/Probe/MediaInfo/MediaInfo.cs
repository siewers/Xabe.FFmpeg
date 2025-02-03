namespace Xabe.FFmpeg;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;
using JetBrains.Annotations;

/// <inheritdoc cref="IMediaInfo" />
[PublicAPI]
public sealed class MediaInfo : IMediaInfo
{
    private MediaInfo(string path)
    {
        Path = path;
    }

    /// <inheritdoc />
    public string Path { get; }

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
    /// <param name="filePath">FullPath to file</param>
    internal async static Task<IMediaInfo> Get(string filePath)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        return await Get(filePath, cancellationTokenSource.Token);
    }

    /// <summary>
    ///     Get MediaInfo from file
    /// </summary>
    /// <param name="filePath">FullPath to file</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async static Task<IMediaInfo> Get(string filePath, CancellationToken cancellationToken)
    {
        var mediaInfo = new MediaInfo(filePath);
        var wrapper = new FFprobeWrapper();
        mediaInfo = await wrapper.SetProperties(mediaInfo, cancellationToken);
        return mediaInfo;
    }

    /// <summary>
    ///     Get MediaInfo from file
    /// </summary>
    /// <param name="fileInfo">FileInfo</param>
    internal async static Task<IMediaInfo> Get(FileInfo fileInfo)
    {
        if (!File.Exists(fileInfo.FullName))
        {
            throw new InvalidInputException($"Input file {fileInfo.FullName} doesn't exists.");
        }

        return await Get(fileInfo.FullName);
    }
}
