namespace Xabe.FFmpeg.Probe;

/// <inheritdoc />
public class Probe : IProbe
{
    /// <inheritdoc />
    public Task<string> Start(string args, CancellationToken cancellationToken = default)
    {
        var wrapper = new FFprobeWrapper();
        return wrapper.Start(args, cancellationToken);
    }

    /// <summary>
    ///     Get new instance of Conversion
    /// </summary>
    /// <returns>IProbe object</returns>
    public static IProbe New()
    {
        return new Probe();
    }
}
