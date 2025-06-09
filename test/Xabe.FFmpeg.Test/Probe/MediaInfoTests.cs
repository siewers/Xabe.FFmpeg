using FluentAssertions;

namespace Xabe.FFmpeg.Test;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Fixtures;
using Xunit;

public class MediaInfoTests(StorageFixture storageFixture, RtspServerFixture rtspServer)
    : IClassFixture<StorageFixture>, IClassFixture<RtspServerFixture>
{
    [Fact]
    public async Task AudioPropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.Mp3);

        mediaInfo.Path.Exists.Should().BeTrue();
        mediaInfo.Path.Extension.Should().Be(FileExtensions.Mp3);
        mediaInfo.Path.Name.Should().Be("audio.mp3");

        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        audioStream.Should().NotBeNull();
        audioStream.Codec.Should().Be("mp3");
        Assert.Equal(13, audioStream.Duration.Seconds);

        mediaInfo.VideoStreams.Should().BeEmpty();

        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Equal(216916, mediaInfo.Size);
    }

    [Fact]
    public async Task GetMultipleStreamsTest()
    {
        var videoInfo = await FFmpeg.GetMediaInfo(Resources.MultipleStream);

        Assert.Single(videoInfo.VideoStreams);
        Assert.Equal(2, videoInfo.AudioStreams.Count());
        Assert.Equal(8, videoInfo.SubtitleStreams.Count());
    }

    [Fact]
    public async Task GetVideoBitrateTest()
    {
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var videoStream = info.VideoStreams.First();

        Assert.Equal(860233, videoStream.Bitrate);
    }

    [Fact]
    public async Task IncorrectFormatTest()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => FFmpeg.GetMediaInfo(Resources.Dll));
    }

    [Fact]
    public async Task Mp4PropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.BunnyMp4);

        mediaInfo.Streams.Should().NotBeEmpty();
    }

    [Fact]
    public async Task MkvPropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        mediaInfo.Path.Exists.Should().BeTrue();
        mediaInfo.Path.Extension.Should().Be(FileExtensions.Mkv);
        mediaInfo.Path.Name.Should().Be("SampleVideo_360x240_1mb.mkv");

        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        audioStream.Should().NotBeNull();
        audioStream.Codec.Should().Be("aac");
        Assert.Equal(1, audioStream.Index);
        Assert.Equal(9, audioStream.Duration.Seconds);

        Assert.Single(mediaInfo.VideoStreams);
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        Assert.Equal(0, videoStream.Index);
        Assert.Equal(25, videoStream.Framerate);
        Assert.Equal(240, videoStream.Height);
        Assert.Equal(320, videoStream.Width);
        Assert.Equal("4:3", videoStream.Ratio);
        videoStream.Codec.Should().Be("h264");
        Assert.Equal(9, videoStream.Duration.Seconds);

        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal(1055721, mediaInfo.Size);
    }

    [Fact]
    public async Task PropertiesTest()
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(Resources.Mp4WithAudio);

        mediaInfo.Path.Exists.Should().BeTrue();
        mediaInfo.Path.Extension.Should().Be(FileExtensions.Mp4);
        mediaInfo.Path.Name.Should().Be("input.mp4");

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
        File.Copy(Resources.Mp4WithAudio, output, true);

        var mediaInfo = await FFmpeg.GetMediaInfo(output);

        mediaInfo.Should().NotBeNull();
        mediaInfo.Path.Extension.Should().Be(FileExtensions.Mp4);
    }

    [Fact]
    public async Task RTSP_NotExistingStream_CancelledAfter30Seconds()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo(@"rtsp://192.168.1.123:554/"));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task RTSP_NotExistingStream_CancelledAfter2Seconds()
    {
        var cancellationTokenSource = new CancellationTokenSource(2000);
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo(@"rtsp://192.168.1.123:554/", cancellationTokenSource.Token));

        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public async Task CalculateFramerate_SloMoVideo_CorrectFramerateIsReturned()
    {
        var info = await FFmpeg.GetMediaInfo(Resources.SloMoMp4);
        var videoStream = info.VideoStreams.First();

        // It does not have to be the same
        Assert.Equal(116, (int)videoStream.Framerate);
        Assert.Equal(3, videoStream.Duration.Seconds);
    }

    [Fact]
    public async Task MediaInfo_SpecialCharactersInName_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var nameWithSpaces = new FileInfo(output).Name;
        output = output.Replace(nameWithSpaces, "Crime d'Amour" + ".mp4");
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .AddParameter("-re", ParameterPosition.PreInput)
                                           .SetOutput(output)
                                           .Start();

        var outputMediaInfo = await FFmpeg.GetMediaInfo(output);
        outputMediaInfo.Streams.Should().NotBeNull();
        conversionResult.Arguments.Should().Contain("Crime d'Amour");
    }

    [Fact]
    public async Task MediaInfo_NameWithSpaces_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var nameWithSpaces = new FileInfo(output).Name.Replace("-", " ");
        output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        _ = await FFmpeg.Conversions.Create()
                        .AddStream(info.VideoStreams.First())
                        .AddParameter("-re", ParameterPosition.PreInput)
                        .SetOutput(output)
                        .Start();

        var outputMediaInfo = await FFmpeg.GetMediaInfo(output);
        outputMediaInfo.Streams.Should().NotBeNull();
    }

    [Fact]
    public async Task MediaInfo_EscapedString_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var nameWithSpaces = new FileInfo(output).Name.Replace("-", " ");
        output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        _ = await FFmpeg.Conversions.Create()
                        .AddStream(info.VideoStreams.First())
                        .AddParameter("-re", ParameterPosition.PreInput)
                        .SetOutput(output)
                        .Start();

        var outputMediaInfo = await FFmpeg.GetMediaInfo($"\"{output}\"");
        outputMediaInfo.Streams.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMediaInfo_RTSP_CorrectDataIsShown()
    {
        await rtspServer.Publish(Resources.BunnyMp4, "bunny2");

        var result = await FFmpeg.GetMediaInfo("rtsp://127.0.0.1:8554/bunny2");

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
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://127.0.0.1:8554/notExisting"));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task GetMediaInfo_NotExistingRtspServer_ThrowException()
    {
        var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo("rtsp://xabe.net/notExisting"));

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public async Task MediaInfo_EscapedString_BasePathDidNotChanged()
    {
        var tempDir = storageFixture.GetTempDirectory();
        var input = Path.Combine(tempDir, "AMD is NOT Ripping Off Intel - WAN Show April 30, 2021_v1.mp4");
        new FileInfo(Resources.BunnyMp4).CopyTo(input, overwrite: true);

        var mediaInfo = await FFmpeg.GetMediaInfo(input);

        mediaInfo.Path.FullName.Should().Be(input);
    }

    [Fact]
    public async Task MediaInfo_EscapedString_BasePathInStreamsDidNotChanged()
    {
        var tempDir = storageFixture.GetTempDirectory();
        var input = Path.Combine(tempDir, "AMD is NOT Ripping Off Intel - WAN Show April 30, 2021_v2.mp4");
        new FileInfo(Resources.BunnyMp4).CopyTo(input, overwrite: true);

        var info = await FFmpeg.GetMediaInfo(input);

        info.VideoStreams.First().Path.Should().Be($"\"{input}\"");
    }
}
