namespace Xabe.FFmpeg;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

    public static Task<IMediaInfo> GetMediaInfo(FileInfo file, CancellationToken cancellationToken = default)
    {
        return GetMediaInfo(file.FullName, cancellationToken);
    }

    /// <summary>
    ///     Get MediaInfo from file
    /// </summary>
    /// <param name="location">FullPath to file</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentException">File does not exist</exception>
    /// <exception cref="TaskCanceledException">Operation takes too long</exception>
    public async static Task<IMediaInfo> GetMediaInfo(string location, CancellationToken cancellationToken = default)
    {
        location = location.Trim('"');

        if (!Uri.TryCreate(location, UriKind.Absolute, out var mediaLocation))
        {
            throw new ArgumentException($"Invalid location: {location}", nameof(location));
        }

        return await MediaInfo.Get(mediaLocation, cancellationToken);
    }

    public static Task<IMediaInfo> GetMediaInfo(Uri location, CancellationToken cancellationToken = default)
    {
        if (!location.IsAbsoluteUri)
        {
            throw new ArgumentException($"Invalid location: {location}", nameof(location));
        }

        return MediaInfo.Get(location, cancellationToken);
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
        _ffmpegExecutableName = ffmpegExecutableName;
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
