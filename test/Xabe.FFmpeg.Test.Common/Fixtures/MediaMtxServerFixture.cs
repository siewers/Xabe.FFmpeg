namespace Xabe.FFmpeg.Test.Common.Fixtures;

using System.Threading.Tasks;
using TestContainers;
using Testcontainers.Xunit;
using Xunit.Abstractions;

public sealed class MediaMtxServerFixture(IMessageSink messageSink)
    : ContainerFixture<MediaMtxServerBuilder, MediaMtxServerContainer>(messageSink)
{
    public async Task Publish(string filePath, string name)
    {
        var parameters = $"-re -stream_loop -1 -i \"{filePath}\" -pix_fmt yuv420p -vsync 1 -vcodec libx264 -r 23.976 -threads 0 -b:v: 1024k -bufsize 1024k -preset veryfast -profile:v baseline -tune film -g 48 -x264opts no-scenecut -acodec aac -b:a 192k -f rtsp rtsp://{Container.Hostname}:8554/{name}";
        _ = FFmpeg.Conversions.Create().AddParameter(parameters).Start();
        await Task.Delay(2000);
    }
}
