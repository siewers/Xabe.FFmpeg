namespace Xabe.FFmpeg.Test.Common;

public static class Resources
{
    public static readonly string FFbinariesInfo = GetResourceFilePath("ffbinaries.json");

    private static string GetResourceFilePath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
    }
}
