namespace Xabe.FFmpeg;

using JetBrains.Annotations;

/// <summary>
///     Stream type
/// </summary>
[PublicAPI]
public enum StreamType
{
    /// <summary>
    ///     Video stream
    /// </summary>
    Video = 0,

    /// <summary>
    ///     Audio stream
    /// </summary>
    Audio = 1,

    /// <summary>
    ///     Subtitle stream
    /// </summary>
    Subtitle = 2,
}
