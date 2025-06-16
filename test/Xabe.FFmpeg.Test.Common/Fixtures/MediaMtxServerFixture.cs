namespace Xabe.FFmpeg.Test.Common.Fixtures;

using TestContainers;
using Testcontainers.Xunit;
using Xunit.Sdk;

public sealed class MediaMtxServerFixture(IMessageSink messageSink)
    : ContainerFixture<MediaMtxServerBuilder, MediaMtxServerContainer>(messageSink)
{
    public Uri GetResourceUri(string name)
    {
        return Container.GetResourceUri(name);
    }

    public async Task<Uri> Publish(string filePath, string name)
    {
        var resourceUri = Container.GetResourceUri(name);
        var parameters = $"-re -stream_loop -1 -i \"{filePath}\" -pix_fmt yuv420p -vsync 1 -vcodec libx264 -r 23.976 -threads 0 -b:v: 1024k -bufsize 1024k -preset veryfast -profile:v baseline -tune film -g 48 -x264opts no-scenecut -acodec aac -b:a 192k -f rtsp {resourceUri}";
        _ = FFmpeg.Conversions.Create().AddParameter(parameters).Start();
        await Task.Delay(2000);
        return resourceUri;
    }
}
