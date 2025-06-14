namespace Xabe.FFmpeg;

/// <inheritdoc />
internal sealed class FilterConfiguration : IFilterConfiguration
{
    /// <inheritdoc />
    public required string FilterType { get; init; }

    /// <inheritdoc />
    public required int StreamNumber { get; init; }

    /// <inheritdoc />
    public Dictionary<string, string> Filters { get; init; } = [];
}
