namespace Xabe.FFmpeg.Exceptions;

/// <inheritdoc />
/// <summary>
///      The exception that is thrown when a FFmpeg cannot find specified hardware accelerator.
/// </summary>
[PublicAPI]
public sealed class HardwareAcceleratorNotFoundException : ConversionExceptionBase
{
    /// <inheritdoc />
    /// <summary>
    ///     The exception that is thrown when a FFmpeg cannot find specified hardware accelerator.
    /// </summary>
    /// <param name="errorMessage">FFmpeg error output</param>
    /// <param name="inputParameters">FFmpeg input parameters</param>
    internal HardwareAcceleratorNotFoundException(string errorMessage, string inputParameters)
        : base(errorMessage, inputParameters)
    {
    }

    internal static HardwareAcceleratorNotFoundException Create(string errorMessage, string inputParameters)
    {
        return new HardwareAcceleratorNotFoundException(errorMessage, inputParameters);
    }
}
