namespace Xabe.FFmpeg.Test.Common.Fixtures.TestContainers;

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

public sealed class MediaMtxServerBuilder(MediaMtxServerConfiguration dockerResourceConfiguration)
    : ContainerBuilder<MediaMtxServerBuilder, MediaMtxServerContainer, MediaMtxServerConfiguration>(dockerResourceConfiguration)
{
    private const string RstpSimpleServerImage = "bluenviron/mediamtx:latest";

    public MediaMtxServerBuilder()
        : this(new MediaMtxServerConfiguration())
    {
        DockerResourceConfiguration = Init().DockerResourceConfiguration;
    }

    protected override MediaMtxServerConfiguration DockerResourceConfiguration { get; } = dockerResourceConfiguration;

    public override MediaMtxServerContainer Build()
    {
        Validate();
        var waitStrategy = Wait.ForUnixContainer();
        var builder = WithWaitStrategy(waitStrategy);
        return new MediaMtxServerContainer(builder.DockerResourceConfiguration);
    }

    protected override MediaMtxServerBuilder Init()
    {
        return base.Init()
                   .WithImage(RstpSimpleServerImage)
                   .WithEnvironment("MTX_RTSPTRANSPORTS", "tcp")
                   .WithPortBinding(8554)
                   .WithAutoRemove(true);
    }

    protected override MediaMtxServerBuilder Clone(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MediaMtxServerConfiguration(resourceConfiguration));
    }

    protected override MediaMtxServerBuilder Clone(IContainerConfiguration resourceConfiguration)
    {
        return Merge(DockerResourceConfiguration, new MediaMtxServerConfiguration(resourceConfiguration));
    }

    protected override MediaMtxServerBuilder Merge(MediaMtxServerConfiguration oldValue, MediaMtxServerConfiguration newValue)
    {
        return new MediaMtxServerBuilder(new MediaMtxServerConfiguration(oldValue, newValue));
    }
}
