namespace Xabe.FFmpeg.Exceptions;

/// <inheritdoc />
/// <summary>
///     The exception that is thrown when a FFmpeg process cannot find suitable output format.
/// </summary>
[PublicAPI]
public sealed class InvalidBitstreamFilterException : ConversionExceptionBase
{
    /// <inheritdoc />
    /// <summary>
    ///     The exception that is thrown when a FFmpeg process cannot find suitable output format.
    /// </summary>
    /// <param name="errorMessage">FFmpeg error output</param>
    /// <param name="inputParameters">FFmpeg error output</param>
    internal InvalidBitstreamFilterException(string errorMessage, string inputParameters)
        : base(errorMessage, inputParameters)
    {
    }

    internal static InvalidBitstreamFilterException Create(string errorMessage, string inputParameters)
    {
        return new InvalidBitstreamFilterException(errorMessage, inputParameters);
    }
}
