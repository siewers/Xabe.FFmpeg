namespace Xabe.FFmpeg;

/// <summary>
///     Defines types of available rotation
/// </summary>
[PublicAPI]
public enum RotateDegrees
{
    /// <summary>
    ///     90 degrees counterclockwise and vertical flip
    /// </summary>
    CounterClockwiseAndFlip = 0,

    /// <summary>
    ///     90 degrees clockwise
    /// </summary>
    Clockwise = 1,

    /// <summary>
    ///     90 degrees counterclockwise
    /// </summary>
    CounterClockwise = 2,

    /// <summary>
    ///     90 degrees counterclockwise and vertical flip
    /// </summary>
    ClockwiseAndFlip = 3,

    /// <summary>
    ///     Rotate video by 180 degrees
    /// </summary>
    Invert = 5,
}
