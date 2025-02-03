namespace Xabe.FFmpeg;

using JetBrains.Annotations;

[PublicAPI]
public class Conversions
{
    /// <summary>
    ///     Get new instance of Conversion
    /// </summary>
    /// <returns>IConversion object</returns>
    public readonly Snippets FromSnippet = new();

    internal Conversions()
    {
    }

    /// <summary>
    ///     Get new instance of Conversion
    /// </summary>
    /// <returns>IConversion object</returns>
    public IConversion New()
    {
        return Conversion.New();
    }
}
