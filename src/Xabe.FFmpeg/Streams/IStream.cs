namespace Xabe.FFmpeg;

using System.Collections.Generic;
using JetBrains.Annotations;

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
    string Codec { get; }

    /// <summary>
    ///     Codec type
    /// </summary>
    StreamType StreamType { get; }

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
