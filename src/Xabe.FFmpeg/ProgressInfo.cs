namespace Xabe.FFmpeg;

[PublicAPI]
public class ProgressInfo
{
    public ProgressInfo()
    {
    }

    public ProgressInfo(long downloadedBytes, long totalBytes)
    {
        DownloadedBytes = downloadedBytes;
        TotalBytes = totalBytes;
    }

    public long DownloadedBytes { get; set; }

    public long TotalBytes { get; set; }
}
