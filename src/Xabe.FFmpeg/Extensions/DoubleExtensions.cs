namespace Xabe.FFmpeg;

using System.Globalization;

internal static class DoubleExtensions
{
    public static string ToFFmpegFormat(this double number, int decimalPlaces = 1)
    {
        return string.Format(CultureInfo.GetCultureInfo("en-US"), $"{{0:N{decimalPlaces}}}", number);
    }
}
