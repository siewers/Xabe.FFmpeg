namespace Xabe.FFmpeg;

using System;
using System.Globalization;
using System.Text.Json;

internal static class TimeSpanParser
{
    public static TimeSpan? Parse(string? duration)
    {
        if (duration is null)
        {
            return null;
        }

        if (double.TryParse(duration, NumberFormatInfo.InvariantInfo, out var seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }

        if (duration.Length > 16)
        {
            duration = duration[..16];
        }

        if (TimeSpan.TryParse(duration, NumberFormatInfo.InvariantInfo, out var timeSpan))
        {
            return timeSpan;
        }

        Console.WriteLine($"Can't parse duration as TimeSpan: {duration}");

        return null;
    }

    public static TimeSpan? GetTimeSpan(this JsonElement element)
    {
        return Parse(element.GetString()!);
    }
}
