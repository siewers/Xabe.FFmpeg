namespace Xabe.FFmpeg.Test.Common.Fixtures;

public sealed class StorageFixture : IAsyncLifetime
{
    public DirectoryInfo TempDirectory { get; private set; } = null!;

    public ValueTask InitializeAsync()
    {
        TempDirectory = Directory.CreateTempSubdirectory("FFmpegTests_");
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        while (TempDirectory.Exists)
        {
            try
            {
                TempDirectory.Delete(recursive: true);
                break;
            }
            catch
            {
                await Task.Delay(500.Milliseconds());
            }
        }
    }

    public FileInfo GetTempFileName(string? extension = null)
    {
        var fileName = Path.ChangeExtension(Path.GetRandomFileName(), extension);
        return new FileInfo(Path.Combine(TempDirectory.FullName, fileName));
    }

    public string GetTempDirectory()
    {
        var path = Path.Combine(TempDirectory.FullName, Path.GetRandomFileName());
        Directory.CreateDirectory(path);
        return path;
    }
}
