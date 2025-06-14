namespace Xabe.FFmpeg.Events;

/// <summary>
///     Video data
/// </summary>
[PublicAPI]
public sealed class VideoDataEventArgs : EventArgs
{
    /// <inheritdoc />
    public VideoDataEventArgs(byte[] data)
    {
        Data = data;
    }

    /// <summary>
    ///     Binary video data
    /// </summary>
    public byte[] Data { get; }
}
