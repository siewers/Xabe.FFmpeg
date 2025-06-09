namespace Xabe.FFmpeg.Exceptions;

/// <inheritdoc />
/// <summary>
///     The exception that is thrown when a FFmpeg process cannot find suitable output format.
/// </summary>
[PublicAPI]
public sealed class FFmpegNoSuitableOutputFormatFoundException : ConversionExceptionBase
{
    /// <inheritdoc />
    /// <summary>
    ///     The exception that is thrown when a FFmpeg process cannot find suitable output format.
    /// </summary>
    /// <param name="errorMessage">FFmpeg error output</param>
    /// <param name="inputParameters">FFmpeg input parameters</param>
    internal FFmpegNoSuitableOutputFormatFoundException(string errorMessage, string inputParameters)
        : base(errorMessage, inputParameters)
    {
    }

    internal static FFmpegNoSuitableOutputFormatFoundException Create(string errorMessage, string inputParameters)
    {
        return new FFmpegNoSuitableOutputFormatFoundException(errorMessage, inputParameters);
    }
}
