namespace Xabe.FFmpeg.Exceptions;

using System;

/// <inheritdoc />
/// <summary>
///     The exception that is thrown when a FFmpeg process return error.
/// </summary>
[PublicAPI]
public class ConversionExceptionBase : Exception
{
    /// <inheritdoc />
    /// <summary>
    ///     The exception that is thrown when an FFmpeg process return error.
    /// </summary>
    /// <param name="message">The FFmpeg error message</param>
    /// <param name="inputParameters">The FFmpeg input parameters</param>
    /// <param name="innerException">The inner exception</param>
    protected internal ConversionExceptionBase(string message, Exception innerException, string inputParameters)
        : base(message, innerException)
    {
        InputParameters = inputParameters;
    }

    /// <inheritdoc />
    /// <summary>
    ///     The exception that is thrown when an FFmpeg process return error.
    /// </summary>
    /// <param name="errorMessage">FFmpeg error output</param>
    /// <param name="inputParameters">FFmpeg input parameters</param>
    internal ConversionExceptionBase(string errorMessage, string inputParameters)
        : base(errorMessage)
    {
        InputParameters = inputParameters;
    }

    /// <summary>
    ///     Gets the FFmpeg input parameters
    /// </summary>
    public string InputParameters { get; }

}

public sealed class GenericConversionException : ConversionExceptionBase
{
    internal GenericConversionException(string message, Exception innerException, string inputParameters)
        : base(message, innerException, inputParameters)
    {
    }

    internal GenericConversionException(string errorMessage, string inputParameters)
        : base(errorMessage, inputParameters)
    {
    }

    public static GenericConversionException Create(string errorMessage, string inputParameters)
    {
        return new GenericConversionException(errorMessage, inputParameters);
    }
}
