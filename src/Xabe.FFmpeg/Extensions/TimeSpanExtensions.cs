namespace Xabe.FFmpeg;

using System;

/// <summary>
///     Extension methods
/// </summary>
internal static class TimeSpanExtensions
{
    /// <summary>
    ///     Return FFmpeg formatted time
    /// </summary>
    /// <param name="duration">The <see cref="TimeSpan" /> to format</param>
    /// <returns>FFmpeg formated time</returns>
    public static string ToFFmpeg(this TimeSpan duration)
    {
        var milliseconds = duration.Milliseconds;
        var seconds = duration.Seconds;
        var minutes = duration.Minutes;
        var hours = (int)duration.TotalHours;

        return $"{hours:D}:{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
    }
}
