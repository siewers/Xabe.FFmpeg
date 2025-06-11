namespace Xabe.FFmpeg.Test.Common.Fixtures.TestContainers;

using System;
using DotNet.Testcontainers.Containers;

public sealed class MediaMtxServerContainer(MediaMtxServerConfiguration configuration)
    : DockerContainer(configuration)
{
    public Uri GetResourceUri(string path) => new(new Uri($"rtsp://{Hostname}:{GetMappedPublicPort(8554)}"), path);
}
