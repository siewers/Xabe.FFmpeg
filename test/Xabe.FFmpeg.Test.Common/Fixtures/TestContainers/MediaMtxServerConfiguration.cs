namespace Xabe.FFmpeg.Test.Common.Fixtures.TestContainers;

using Docker.DotNet.Models;
using DotNet.Testcontainers.Configurations;

public sealed class MediaMtxServerConfiguration : ContainerConfiguration
{
    public MediaMtxServerConfiguration()
    {
    }

    public MediaMtxServerConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    public MediaMtxServerConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    public MediaMtxServerConfiguration(MediaMtxServerConfiguration oldValue, MediaMtxServerConfiguration newValue)
        : base(oldValue, newValue)
    {
    }
}
