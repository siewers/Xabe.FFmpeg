namespace Xabe.FFmpeg.Events;

/// <summary>
///     Info about conversion progress
/// </summary>
/// <param name="sender">Sender</param>
/// <param name="args">Conversion info</param>
[PublicAPI]
public delegate void ConversionProgressEventHandler(object sender, ConversionProgressEventArgs args);
