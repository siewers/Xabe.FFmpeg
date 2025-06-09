namespace Xabe.FFmpeg.Events;

/// <summary>
///     Info about conversion progress
/// </summary>
/// <param name="sender">Sender</param>
/// <param name="args">Video data</param>
[PublicAPI]
public delegate void VideoDataEventHandler(object sender, VideoDataEventArgs args);
