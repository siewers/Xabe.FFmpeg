namespace Xabe.FFmpeg;

/// <summary>
///     Base stream class
/// </summary>
[PublicAPI]
public interface IStream
{
    /// <summary>
    ///     File source of stream
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     Index of stream
    /// </summary>
    int Index { get; }

    /// <summary>
    ///     Format
    /// </summary>
    string? Codec { get; }

    /// <summary>
    ///     Gets the language of stream
    /// </summary>
    string? Language { get; }

    /// <summary>
    ///     Gets the duration of the stream
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    ///     Gets the bitrate of the stream in bits per second
    /// </summary>
    long Bitrate { get; }

    /// <summary>
    ///     Gets the type of stream
    /// </summary>
    StreamType StreamType { get; }

    /// <summary>
    ///     Gets the title of the stream.
    /// </summary>
    string? Title { get; }

    /// <summary>
    ///     Get a value indicating whether the stream is default or not.
    /// </summary>
    bool? IsDefault { get; }

    /// <summary>
    ///     Gets a value indicating whether the stream is forced or not.
    /// </summary>
    bool? IsForced { get; }

    /// <summary>
    ///     Build FFmpeg arguments for input
    /// </summary>
    /// <returns>Arguments</returns>
    string BuildParameters(ParameterPosition forPosition);

    /// <summary>
    ///     Get stream input
    /// </summary>
    /// <returns>Input path</returns>
    IEnumerable<string> GetSource();
}
