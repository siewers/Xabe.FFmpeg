namespace Xabe.FFmpeg.Test;

public class MediaInfoTests(StorageFixture storageFixture, MediaMtxServerFixture rtspServer)
    : IClassFixture<StorageFixture>, IClassFixture<MediaMtxServerFixture>
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task AudioPropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.Mp3, _cancellationToken);

        var mediaFile = new FileInfo(mediaInfo.Location.LocalPath);
        mediaFile.Exists.Should().BeTrue();
        mediaFile.Extension.Should().Be(FileExtensions.Mp3);
        mediaFile.Name.Should().Be("audio.mp3");

        var audioStream = mediaInfo.AudioStreams.Should().ContainSingle().Subject;
        audioStream.Should().NotBeNull();
        audioStream.Codec.Should().Be("mp3");
        audioStream.Duration.Should().Be(13.Seconds().And(536.Milliseconds()));

        mediaInfo.VideoStreams.Should().BeEmpty();

        mediaInfo.Duration.Should().Be(audioStream.Duration);
        mediaInfo.Size.Should().Be(216916);
    }

    [Fact]
    public async Task GetMultipleStreamsTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.MultipleStream, _cancellationToken);

        mediaInfo.VideoStreams.Should().ContainSingle();
        mediaInfo.AudioStreams.Should().HaveCount(2);
        mediaInfo.SubtitleStreams.Should().HaveCount(8);
    }

    [Fact]
    public async Task GetVideoBitrateTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _cancellationToken);

        mediaInfo.VideoStreams.Should().ContainSingle()
                 .Which.Bitrate.Should().Be(860233);
    }

    [Fact]
    public async Task IncorrectFormatTest()
    {
        await FluentActions.Awaiting(() => FFmpeg.GetMediaInfo(Resources.Dll, _cancellationToken))
                           .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Mp4PropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.BunnyMp4, _cancellationToken);

        mediaInfo.Streams.Should().NotBeEmpty();
    }

    [Fact]
    public async Task MkvPropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _cancellationToken);

        var mediaFile = new FileInfo(mediaInfo.Location.LocalPath);
        mediaFile.Exists.Should().BeTrue();
        mediaFile.Extension.Should().Be(FileExtensions.Mkv);
        mediaFile.Name.Should().Be("SampleVideo_360x240_1mb.mkv");

        var expectedDuration = 9.Seconds().And(818.Milliseconds());
        mediaInfo.AudioStreams.Should().ContainSingle()
                 .Which.Should().Satisfy<IAudioStream>(stream =>
                                                       {
                                                           stream.Should().NotBeNull();
                                                           stream.Codec.Should().Be("aac");
                                                           stream.Index.Should().Be(1);
                                                           stream.Duration.Should().Be(expectedDuration);
                                                       }
                                                      );

        mediaInfo.VideoStreams.Should().ContainSingle()
                 .Which.Should().Satisfy<IVideoStream>(stream =>
                                                       {
                                                           stream.Should().NotBeNull();
                                                           stream.Codec.Should().Be("h264");
                                                           stream.Index.Should().Be(0);
                                                           stream.Framerate.Should().Be(25);
                                                           stream.Height.Should().Be(240);
                                                           stream.Width.Should().Be(320);
                                                           stream.Ratio.Should().Be("4:3");
                                                           stream.Duration.Should().Be(expectedDuration);
                                                       }
                                                      );

        mediaInfo.Duration.Should().Be(expectedDuration);
        mediaInfo.Size.Should().Be(1055721);
    }

    [Fact]
    public async Task PropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.Mp4WithAudio, _cancellationToken);

        var mediaFile = new FileInfo(mediaInfo.Location.LocalPath);
        mediaFile.Exists.Should().BeTrue();
        mediaFile.Extension.Should().Be(FileExtensions.Mp4);
        mediaFile.Name.Should().Be("input.mp4");

        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        audioStream.Should().NotBeNull();
        audioStream.Codec.Should().Be("aac");
        Assert.Equal(13, audioStream.Duration.Seconds);

        Assert.Single(mediaInfo.VideoStreams);
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        Assert.Equal(25, videoStream.Framerate);
        Assert.Equal(720, videoStream.Height);
        Assert.Equal(1280, videoStream.Width);
        videoStream.Ratio.Should().Be("16:9");
        videoStream.Codec.Should().Be("h264");
        Assert.Equal(13, videoStream.Duration.Seconds);

        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Equal(2107842, mediaInfo.Size);
    }

    [Theory]
    [InlineData("檔")]
    [InlineData("אספירין")]
    [InlineData("एस्पिरि")]
    [InlineData("阿司匹林")]
    [InlineData("アセチルサリチル酸")]
    public async Task GetMediaInfo_NonUTF8CharactersInPath(string path)
    {
        var output = storageFixture.GetTempFileName($"{path}{FileExtensions.Mp4}");
        File.Copy(Resources.Mp4WithAudio, output.FullName, true);

        var mediaInfo = await FFmpeg.GetMediaInfo(output, _cancellationToken);

        mediaInfo.Should().NotBeNull();
        Path.GetExtension(mediaInfo.Location.LocalPath).Should().Be(FileExtensions.Mp4);
    }

    [Fact]
    public async Task RTSP_NotExistingStream_CanceledAfter30Seconds()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://192.168.1.123:554/", _cancellationToken));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task RTSP_NotExistingStream_CanceledAfter2Seconds()
    {
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, new CancellationTokenSource(2.Seconds()).Token);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://192.168.1.123:554/", cancellationTokenSource.Token));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task CalculateFramerate_SloMoVideo_CorrectFramerateIsReturned()
    {
        var info = await FFmpeg.GetMediaInfo(Resources.SloMoMp4, _cancellationToken);
        var videoStream = info.VideoStreams.First();

        // It does not have to be the same
        Assert.Equal(116, (int)videoStream.Framerate);
        Assert.Equal(3, videoStream.Duration.Seconds);
    }

    [Fact]
    public async Task MediaInfo_SpecialCharactersInName_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        output = output.Replace(output.Name, "Crime d'Amour" + ".mp4");
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _cancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .AddParameter("-re", ParameterPosition.PreInput)
                                           .SetOutput(output.FullName)
                                           .Start(_cancellationToken);

        var outputMediaInfo = await FFmpeg.GetMediaInfo(output, _cancellationToken);
        outputMediaInfo.Streams.Should().NotBeNull();
        conversionResult.Arguments.Should().Contain("Crime d'Amour");
    }

    [Fact]
    public async Task MediaInfo_NameWithSpaces_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var nameWithSpaces = output.Name.Replace("-", " ");
        output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _cancellationToken);
        await FFmpeg.Conversions.Create()
                    .AddStream(info.VideoStreams.First())
                    .AddParameter("-re", ParameterPosition.PreInput)
                    .SetOutput(output.FullName)
                    .Start(_cancellationToken);

        var outputMediaInfo = await FFmpeg.GetMediaInfo(output, _cancellationToken);
        outputMediaInfo.Streams.Should().NotBeNull();
    }

    [Fact]
    public async Task MediaInfo_EscapedString_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var nameWithSpaces = output.Name.Replace("-", " ");
        output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _cancellationToken);
        await FFmpeg.Conversions.Create()
                    .AddStream(info.VideoStreams.First())
                    .AddParameter("-re", ParameterPosition.PreInput)
                    .SetOutput(output.FullName)
                    .Start(_cancellationToken);

        var outputMediaInfo = await FFmpeg.GetMediaInfo($"\"{output}\"", _cancellationToken);
        outputMediaInfo.Streams.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMediaInfo_RTSP_CorrectDataIsShown()
    {
        var resourceUri = await rtspServer.Publish(Resources.BunnyMp4, "bunny2");

        var result = await FFmpeg.GetMediaInfo(resourceUri, _cancellationToken);

        Assert.Single(result.VideoStreams);
        Assert.Single(result.AudioStreams);
        result.SubtitleStreams.Should().BeEmpty();
        result.VideoStreams.First().Codec.Should().Be("h264");
        Assert.Equal(23.976, result.VideoStreams.First().Framerate);
        Assert.Equal(640, result.VideoStreams.First().Width);
        Assert.Equal(360, result.VideoStreams.First().Height);
        result.AudioStreams.First().Codec.Should().Be("aac");
    }

    [Fact]
    public async Task GetMediaInfo_StreamDoesNotExist_ThrowException()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://127.0.0.1:8554/notExisting", _cancellationToken));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task GetMediaInfo_NotExistingRtspServer_ThrowException()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://xabe.net/notExisting", _cancellationToken));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task MediaInfo_EscapedString_BasePathDidNotChanged()
    {
        var tempDir = storageFixture.GetTempDirectory();
        var input = Path.Combine(tempDir, "AMD is NOT Ripping Off Intel - WAN Show April 30, 2021_v1.mp4");
        File.Copy(Resources.BunnyMp4, input, true);

        var mediaInfo = await FFmpeg.GetMediaInfo(input, _cancellationToken);

        mediaInfo.Location.OriginalString.Should().Be(input);
    }

    [Fact]
    public async Task MediaInfo_EscapedString_BasePathInStreamsDidNotChanged()
    {
        var tempDir = storageFixture.GetTempDirectory();
        var input = Path.Combine(tempDir, "AMD is NOT Ripping Off Intel - WAN Show April 30, 2021_v2.mp4");
        File.Copy(Resources.BunnyMp4, input, true);

        var info = await FFmpeg.GetMediaInfo(input, _cancellationToken);

        info.VideoStreams.First().Path.Should().Be($"\"{input}\"");
    }
}
