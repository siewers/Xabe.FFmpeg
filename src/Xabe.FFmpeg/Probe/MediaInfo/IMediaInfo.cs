namespace Xabe.FFmpeg.Probe;

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

/// <summary>
///     Information about media file
/// </summary>
[PublicAPI]
public interface IMediaInfo
{
    /// <summary>
    ///     Source info
    /// </summary>
    Uri Location { get; }

    /// <summary>
    ///     Date and Time when the media was created
    /// </summary>
    DateTime? CreationTime { get; }

    /// <summary>
    ///     Size of file
    /// </summary>
    long Size { get; }

    /// <summary>
    ///     Duration of media
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    ///     All file streams
    /// </summary>
    IEnumerable<IStream> Streams { get; }

    /// <summary>
    ///     Video streams
    /// </summary>
    IEnumerable<IVideoStream> VideoStreams { get; }

    /// <summary>
    ///     Audio streams
    /// </summary>
    IEnumerable<IAudioStream> AudioStreams { get; }

    /// <summary>
    ///     Audio streams
    /// </summary>
    IEnumerable<ISubtitleStream> SubtitleStreams { get; }
}
