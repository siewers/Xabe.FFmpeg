namespace Xabe.FFmpeg.Events;

/// <summary>
///     Conversion information
/// </summary>
[PublicAPI]
public sealed class ConversionProgressEventArgs : EventArgs
{
    /// <inheritdoc />
    public ConversionProgressEventArgs(TimeSpan timeSpan, TimeSpan totalTime, int processId)
    {
        Duration = timeSpan;
        TotalLength = totalTime;
        ProcessId = processId;
    }

    /// <summary>
    ///     Current processing time
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    ///     Input movie length
    /// </summary>
    public TimeSpan TotalLength { get; }

    /// <summary>
    ///     Process id
    /// </summary>
    public long ProcessId { get; }

    /// <summary>
    ///     Percent of conversion
    /// </summary>
    public int Percent => (int)(Math.Round(Duration.TotalSeconds / TotalLength.TotalSeconds, 2) * 100);
}
