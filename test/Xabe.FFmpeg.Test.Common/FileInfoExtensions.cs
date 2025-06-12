namespace Xabe.FFmpeg.Test.Common;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

public static class FileInfoExtensions
{
    public static Task<byte[]> ReadAllBytesAsync(this FileInfo fileInfo, CancellationToken cancellationToken = default)
    {
        return File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken);
    }
}
