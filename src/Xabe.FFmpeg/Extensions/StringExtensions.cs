namespace Xabe.FFmpeg;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

[PublicAPI]
public static class StringExtensions
{
    [return: NotNullIfNotNull(nameof(output))]
    public static string? Escape([NotNullIfNotNull(nameof(output))] this string? output)
    {
        if (output == null)
        {
            return output;
        }

        if ((output.Last() == '\"' && output.First() == '\"') || (output.Last() == '\'' && output.First() == '\''))
        {
            output = output.Substring(1, output.Length - 2);
        }

        output = $"\"{output}\"";
        return output;
    }

    [return: NotNullIfNotNull(nameof(output))]
    public static string? Unescape(this string? output)
    {
        if (output == null || output.Length < 2)
        {
            return output;
        }

        if ((output.Last() == '\"' && output.First() == '\"') || (output.Last() == '\'' && output.First() == '\''))
        {
            return output.Substring(1, output.Length - 2);
        }

        return output;
    }
}
