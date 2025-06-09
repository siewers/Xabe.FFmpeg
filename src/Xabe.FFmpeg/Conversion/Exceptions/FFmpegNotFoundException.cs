namespace Xabe.FFmpeg.Exceptions;

using System.IO;

/// <inheritdoc />
/// <summary>
///     The exception that is thrown when a FFmpeg process return error.
/// </summary>
[PublicAPI]
public sealed class FFmpegNotFoundException : FileNotFoundException
{
    /// <inheritdoc />
    /// <summary>
    ///     The exception that is thrown when a FFmpeg executables cannot be found.
    /// </summary>
    /// <param name="errorMessage">FFmpeg error output</param>
    internal FFmpegNotFoundException(string errorMessage) : base(errorMessage)
    {
    }
}
