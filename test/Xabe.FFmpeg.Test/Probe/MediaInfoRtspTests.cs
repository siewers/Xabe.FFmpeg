namespace Xabe.FFmpeg.Test;

public class MediaInfoRtspTests(MediaMtxServerFixture rtspServer) : IClassFixture<MediaMtxServerFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetMediaInfo_RTSP_CorrectDataIsShown()
    {
        var resourceUri = await rtspServer.Publish(Resources.BunnyMp4, "bunny2");

        var result = await FFmpeg.GetMediaInfo(resourceUri, _testCancellationToken);

        Assert.Single(result.VideoStreams);
        Assert.Single(result.AudioStreams);
        result.SubtitleStreams.Should().BeEmpty();
        result.VideoStreams.First().Codec.Should().Be("h264");
        Assert.Equal(expected: 23.976, result.VideoStreams.First().Framerate);
        Assert.Equal(expected: 640, result.VideoStreams.First().Width);
        Assert.Equal(expected: 360, result.VideoStreams.First().Height);
        result.AudioStreams.First().Codec.Should().Be("aac");
    }

    [Fact]
    public async Task GetMediaInfo_NotExistingRtspServer_ThrowException()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://xabe.net/notExisting", _testCancellationToken));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task RTSP_NotExistingStream_CanceledAfter30Seconds()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://192.168.1.123:554/", _testCancellationToken));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task RTSP_NotExistingStream_CanceledAfter2Seconds()
    {
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_testCancellationToken, new CancellationTokenSource(2.Seconds()).Token);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://192.168.1.123:554/", cancellationTokenSource.Token));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task GetMediaInfo_StreamDoesNotExist_ThrowException()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://127.0.0.1:8554/notExisting", _testCancellationToken));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }
}
