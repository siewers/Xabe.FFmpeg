namespace Xabe.FFmpeg;

using System;

/// <inheritdoc />
internal class ConversionResult : IConversionResult
{
    /// <inheritdoc />
    public required DateTime StartTime { get; init; }

    /// <inheritdoc />
    public required DateTime EndTime { get; init; }

    /// <inheritdoc />
    public required TimeSpan Duration { get; init; }

    /// <inheritdoc />
    public required string Arguments { get; init; }

    public required string OutputLog { get; init; }
}
