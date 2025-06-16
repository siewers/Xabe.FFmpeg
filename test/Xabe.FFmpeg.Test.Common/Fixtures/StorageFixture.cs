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
                TempDirectory.Delete(true);
                break;
            }
            catch
            {
                await Task.Delay(500.Milliseconds());
            }
        }
    }

    public MediaLocationBuilder CreateMediaLocation()
    {
        return new MediaLocationBuilder(this);
    }

    public string GetTempDirectory()
    {
        var path = Path.Combine(TempDirectory.FullName, Path.GetRandomFileName());
        Directory.CreateDirectory(path);
        return path;
    }

    public sealed class MediaLocationBuilder(StorageFixture storageFixture)
    {
        private MediaLocation? _builtMediaLocation;
        private string? _extension;
        private string? _fileName;

        public MediaLocationBuilder WithFileName(string fileName)
        {
            _fileName = fileName;
            return this;
        }

        public MediaLocationBuilder WithExtension(string extension)
        {
            _extension = extension;
            return this;
        }

        public static implicit operator MediaLocation(MediaLocationBuilder builder)
        {
            return builder.Build();
        }

        public static implicit operator string(MediaLocationBuilder builder)
        {
            return builder.Build();
        }

        public override string ToString()
        {
            return Build();
        }

        private MediaLocation Build()
        {
            var filePath = Path.Combine(storageFixture.TempDirectory.FullName, Path.ChangeExtension(_fileName ?? Path.GetRandomFileName(), _extension));
            _builtMediaLocation ??= MediaLocation.Create(filePath);
            return _builtMediaLocation.Value;
        }
    }
}
