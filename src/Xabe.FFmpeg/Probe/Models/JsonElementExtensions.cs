namespace Xabe.FFmpeg;

using System.Text.Json;

internal static class JsonElementExtensions
{
    public static JsonElement? GetPropertyValueOrDefault(this JsonElement jsonElement, string propertyName)
    {
        return jsonElement.TryGetProperty(propertyName, out var property) ? property : null;
    }
}
