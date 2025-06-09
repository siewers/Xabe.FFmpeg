namespace Xabe.FFmpeg.Exceptions;

/// <inheritdoc />
/// <summary>
///     The exception that is thrown when a FFmpeg cannot find specified hardware accelerator.
/// </summary>
[PublicAPI]
public sealed class UnknownDecoderException : ConversionExceptionBase
{
    /// <inheritdoc />
    /// <summary>
    ///     The exception that is thrown when a FFmpeg cannot find a codec to decode the file.
    /// </summary>
    /// <param name="errorMessage">FFmpeg error output</param>
    /// <param name="inputParameters">FFmpeg input parameters</param>
    internal UnknownDecoderException(string errorMessage, string inputParameters)
        : base(errorMessage, inputParameters)
    {
    }

    internal static UnknownDecoderException Create(string errorMessage, string inputParameters)
    {
        return new UnknownDecoderException(errorMessage, inputParameters);
    }
}
