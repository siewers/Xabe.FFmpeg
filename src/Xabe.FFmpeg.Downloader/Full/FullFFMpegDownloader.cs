namespace Xabe.FFmpeg.Downloader;

using System;
using System.Threading.Tasks;

internal class FullFFmpegDownloader : FFmpegDownloaderBase
{
    internal FullFFmpegDownloader()
    {
    }

    internal FullFFmpegDownloader(IOperatingSystemProvider operatingSystemProvider)
        : base(operatingSystemProvider)
    {
    }

    private string GenerateLink()
    {
        switch (OperatingSystemProvider.GetOperatingSystem())
        {
            case OperatingSystem.Windows64:
                return "https://xabe.net/ffmpeg/versions/ffmpeg-latest-win64-static.zip";
            case OperatingSystem.Windows32:
                return "https://xabe.net/ffmpeg/versions/ffmpeg-latest-win32-static.zip";
            case OperatingSystem.Osx64:
                return "https://xabe.net/ffmpeg/versions/ffmpeg-latest-macos64-static.zip";
            default:
                throw new NotSupportedException($"The automated download of the full FFmpeg package is not supported for the current Operation System: {OperatingSystemProvider.GetOperatingSystem()}.");
        }
    }

    public override async Task GetLatestVersion(string path, IProgress<ProgressInfo> progress = null, int retries = DefaultMaxRetries)
    {
        if (!CheckIfFilesExist(path))
        {
            return;
        }

        var link = GenerateLink();
        var fullPackZip = await DownloadFile(link, progress, retries);

        Extract(fullPackZip, path ?? ".");
    }

    protected override void Extract(string ffMpegZipPath, string destinationDir)
    {
        Extract(ffMpegZipPath, destinationDir, item => item.FullName.Contains("bin"), item => item.Name);
    }
}
