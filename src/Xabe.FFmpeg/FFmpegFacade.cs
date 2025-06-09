namespace Xabe.FFmpeg;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Probe;

/// <summary>
///     Wrapper for FFmpeg
/// </summary>
public abstract partial class FFmpeg
{
    /// <summary>
    ///     Get new instance of Conversion
    /// </summary>
    /// <returns>IConversion object</returns>
    public static Conversions Conversions = new();

    /// <summary>
    ///     Directory containing FFmpeg and FFprobe
    /// </summary>
    public static string? ExecutablesPath { get; private set; }

    /// <summary>
    ///     Filtering method for FFmpeg and FFprobe file lookup
    /// </summary>
    public static FileNameFilterMethod FilterMethod { get; private set; }

    /// <summary>
    ///     Select if filtering method should be case-sensitive
    ///     This will be used to compare file names
    /// </summary>
    public static IFormatProvider FormatProvider { get; private set; } = CultureInfo.InvariantCulture;

    /// <summary>
    ///     Get MediaInfo from file
    /// </summary>
    /// <param name="filePath">FullPath to file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="ArgumentException">File does not exist</exception>
    /// <exception cref="TaskCanceledException">Operation takes too long</exception>
    public async static Task<IMediaInfo> GetMediaInfo(string filePath, CancellationToken cancellationToken = default)
    {
        return await MediaInfo.Get(new FileInfo(filePath), cancellationToken);
    }

    /// <summary>
    ///     Set path to the directory containing FFmpeg and FFprobe
    /// </summary>
    /// <param name="directoryWithFFmpegAndFFprobe"></param>
    /// <param name="ffmpegExecutableName">Name of FFmpeg executable name</param>
    /// <param name="ffprobeExecutableName">Name of FFprobe executable name</param>
    /// <param name="filteringMethod">Select method to compare file names</param>
    /// <param name="formatProvider">Select if filter should be Case Sensitive</param>
    [MemberNotNull(nameof(FilterMethod), nameof(FormatProvider))]
    public static void SetExecutablesPath(string? directoryWithFFmpegAndFFprobe, string ffmpegExecutableName = "ffmpeg", string ffprobeExecutableName = "ffprobe", FileNameFilterMethod filteringMethod = FileNameFilterMethod.Contains, IFormatProvider? formatProvider = null)
    {
        ExecutablesPath = directoryWithFFmpegAndFFprobe == null ? null : new DirectoryInfo(directoryWithFFmpegAndFFprobe).FullName;
        ArgumentException.ThrowIfNullOrWhiteSpace(ffmpegExecutableName, nameof(ffmpegExecutableName));
        ArgumentException.ThrowIfNullOrWhiteSpace(ffprobeExecutableName, nameof(ffprobeExecutableName));
        _ffmpegExecutableName =  ffmpegExecutableName;
        _ffprobeExecutableName = ffprobeExecutableName;

        FilterMethod = filteringMethod;
        FormatProvider = formatProvider ?? CultureInfo.InvariantCulture;
    }

    /// <summary>
    ///     Get available audio and video devices (like cams or mics)
    /// </summary>
    /// <returns>List of available devices</returns>
    internal async static Task<Device[]> GetAvailableDevices()
    {
        return await Conversion.GetAvailableDevices();
    }
}
