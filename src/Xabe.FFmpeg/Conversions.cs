namespace Xabe.FFmpeg;

[PublicAPI]
public sealed class Conversions
{
    internal Conversions()
    {
    }

    /// <summary>
    ///     Get new instance of Conversion
    /// </summary>
    public Snippets FromSnippet { get; } = new();

    /// <summary>
    ///     Get new instance of Conversion
    /// </summary>
    /// <returns>IConversion object</returns>
    public IConversion Create()
    {
        return Conversion.Create();
    }
}
