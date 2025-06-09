namespace Xabe.FFmpeg.Downloader.Extensions;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

[PublicAPI]
public static class StreamExtensions
{
    public async static Task CopyToAsync(this Stream source, Stream destination, long contentLength, int bufferSize, IProgress<ProgressInfo>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);

        if (!source.CanRead)
        {
            throw new ArgumentException("Has to be readable", nameof(source));
        }

        if (!destination.CanWrite)
        {
            throw new ArgumentException("Has to be writable", nameof(destination));
        }

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(new ProgressInfo(totalBytesRead, contentLength));
        }
    }
}
