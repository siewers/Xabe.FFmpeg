namespace Xabe.FFmpeg.Test;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Fixtures;
using FluentAssertions;
using FluentAssertions.Extensions;

public class VideoSnippetsTests(StorageFixture storageFixture, MediaMtxServerFixture rtspServer)
    : IClassFixture<StorageFixture>, IClassFixture<MediaMtxServerFixture>
{
    public static TheoryData<string, string, int, int, int, string> JoinFiles => new()
                                                                                 {
                                                                                     { Resources.MkvWithAudio, Resources.Mp4WithAudio, 23, 1280, 720, "16:9" },
                                                                                     { Resources.MkvWithAudio, Resources.MkvWithAudio, 19, 320, 240, "4:3" },
                                                                                     { Resources.MkvWithAudio, Resources.Mp4, 23, 1280, 720, "16:9" },
                                                                                 };

    [Theory]
    [MemberData(nameof(JoinFiles))]
    public async Task Concatenate_Test(string firstFile, string secondFile, int duration, int width, int height, string ratio)
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);

        var result = await (await FFmpeg.Conversions.FromSnippet.Concatenate(output, firstFile, secondFile)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(duration, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.Equal(width, videoStream.Width);
        Assert.Equal(height, videoStream.Height);
        Assert.Contains($"-aspect {ratio}", result.Arguments);
    }

    [Fact]
    public async Task ChangeSizeTest()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
        var input = Resources.MkvWithAudio;
        _ = await (await FFmpeg.Conversions.FromSnippet.ChangeSize(input, output, 640, 360))
            .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Single(mediaInfo.VideoStreams);
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        Assert.Equal(640, videoStream.Width);
        Assert.Equal(360, videoStream.Height);
    }

    [Fact]
    public async Task ExtractVideo()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var output = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(Resources.Mp4WithAudio));
        _ = await (await FFmpeg.Conversions.FromSnippet.ExtractVideo(Resources.Mp4WithAudio, output))
            .Start(cancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(output, cancellationToken);
        Assert.Single(mediaInfo.VideoStreams);
        mediaInfo.AudioStreams.Should().BeEmpty();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        videoStream.Codec.Should().Be("h264");
    }

    [Fact]
    public async Task SnapshotInvalidArgumentTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Png);
        await Assert.ThrowsAsync<ArgumentException>(async () => await (await FFmpeg.Conversions.FromSnippet.Snapshot(Resources.Mp4WithAudio, output.FullName, TimeSpan.FromSeconds(999))).Start());
    }

    [Theory]
    [InlineData(FileExtensions.Png, 1825653)]
    [InlineData(FileExtensions.Jpg, 84461)]
    public async Task SnapshotTest(string extension, long expectedLength)
    {
        var output = storageFixture.GetTempFileName(extension);
        _ = await (await FFmpeg.Conversions.FromSnippet.Snapshot(Resources.Mp4WithAudio, output.FullName, TimeSpan.FromSeconds(0)))
            .Start();

        output.Exists.Should().BeTrue();
        // It does not have to be the same
        Assert.Equal(expectedLength / 10, (await output.ReadAllBytesAsync()).LongLength / 10);
    }

    [Fact]
    public async Task SplitVideoTest()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Split(Resources.Mp4WithAudio, output, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(8)))
            .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        audioStream.Codec.Should().Be("aac");
        videoStream.Codec.Should().Be("h264");
        Assert.Equal(TimeSpan.FromSeconds(8), audioStream.Duration);
        Assert.Equal(TimeSpan.FromSeconds(8), videoStream.Duration);
        Assert.Equal(TimeSpan.FromSeconds(8), mediaInfo.Duration);
    }

    [Fact]
    public async Task WatermarkTest()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
        var result = await (await FFmpeg.Conversions.FromSnippet.SetWatermark(Resources.Mp4WithAudio, output, Resources.PngSample, Position.Center))
            .Start();

        result.Arguments.Should().Contain("overlay=");
        result.Arguments.Should().Contain(Resources.Mp4WithAudio);
        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        audioStream.Codec.Should().Be("aac");
        videoStream.Codec.Should().Be("h264");
    }

    [Fact]
    public async Task SaveM3U8Stream_Https_EverythingWorks()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), "mkv");
        var uri = new Uri("https://bitdash-a.akamaihd.net/content/MI201109210084_1/m3u8s/f08e80da-bf1d-4e3d-8899-f0f6155f6efa.m3u8");

        var exception = await Record.ExceptionAsync(async () => await (await FFmpeg.Conversions.FromSnippet.SaveM3U8Stream(uri, output, TimeSpan.FromSeconds(1)))
                                                        .Start()
                                                   );

        exception.Should().BeNull();
    }

    [Fact]
    public async Task SaveM3U8Stream_Http_EverythingWorks()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), "mkv");
        var uri = new Uri("http://bitdash-a.akamaihd.net/content/MI201109210084_1/m3u8s/f08e80da-bf1d-4e3d-8899-f0f6155f6efa.m3u8");

        var exception = await Record.ExceptionAsync(async () => await (await FFmpeg.Conversions.FromSnippet.SaveM3U8Stream(uri, output, TimeSpan.FromSeconds(1)))
                                                        .Start()
                                                   );

        exception.Should().BeNull();
    }

    [Fact]
    public async Task SaveM3U8Stream_NotExisting_ExceptionIsThrown()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), "mkv");
        var uri = new Uri("http://www.bitdash-a.akamaihd.net/notexisting.m3u8");

        var exception = await Record.ExceptionAsync(async () => await (await FFmpeg.Conversions.FromSnippet.SaveM3U8Stream(uri, output, TimeSpan.FromSeconds(1)))
                                                        .Start()
                                                   );

        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task BasicConversion_InputFileWithSubtitles_SkipSubtitles()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithSubtitles, output.FullName)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        videoStream.Codec.Should().Be("h264");
        audioStream.Codec.Should().Be("aac");
        mediaInfo.SubtitleStreams.Should().BeEmpty();
        Assert.Equal(25, videoStream.Framerate);
    }

    [Fact]
    public async Task BasicConversion_InputFileWithSubtitles_SkipSubtitlesWithParameter()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithSubtitles, output.FullName, false)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        videoStream.Codec.Should().Be("h264");
        audioStream.Codec.Should().Be("aac");
        mediaInfo.SubtitleStreams.Should().BeEmpty();
        Assert.Equal(25, videoStream.Framerate);
    }

    [Fact]
    public async Task BasicConversion_InputFileWithSubtitles_KeepSubtitles()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithSubtitles, output.FullName, true)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        Assert.Equal(2, mediaInfo.SubtitleStreams.Count());
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        videoStream.Codec.Should().Be("h264");
        audioStream.Codec.Should().Be("aac");
        Assert.Equal(25, videoStream.Framerate);
    }

    [Theory]
    [InlineData(VideoCodec.hevc, AudioCodec.aac, SubtitleCodec.mov_text)]
    [InlineData(VideoCodec.h264, AudioCodec.aac, SubtitleCodec.mov_text)]
    public async Task BasicTranscode_InputFileWithSubtitles_KeepSubtitles(VideoCodec videoCodec, AudioCodec audioCodec, SubtitleCodec subtitleCodec)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Transcode(Resources.MkvWithSubtitles, output.FullName, videoCodec, audioCodec, subtitleCodec, true)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        Assert.Equal(2, mediaInfo.SubtitleStreams.Count());
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        videoStream.Codec.Should().Be(videoCodec.ToStringFast());
        audioStream.Codec.Should().Be(audioCodec.ToStringFast());
        Assert.Equal(25, videoStream.Framerate);
    }

    [Theory]
    [InlineData(VideoCodec.hevc, AudioCodec.aac, SubtitleCodec.copy)]
    [InlineData(VideoCodec.h264, AudioCodec.aac, SubtitleCodec.copy)]
    public async Task BasicTranscode_InputFileWithSubtitles_SkipSubtitles(VideoCodec videoCodec, AudioCodec audioCodec, SubtitleCodec subtitleCodec)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Transcode(Resources.MkvWithSubtitles, output.FullName, videoCodec, audioCodec, subtitleCodec)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        mediaInfo.SubtitleStreams.Should().BeEmpty();
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        videoStream.Codec.Should().Be(videoCodec.ToStringFast());
        audioStream.Codec.Should().Be(audioCodec.ToStringFast());
        Assert.Equal(25, videoStream.Framerate);
    }

    [Theory]
    [InlineData(VideoCodec.hevc, AudioCodec.aac, SubtitleCodec.copy)]
    [InlineData(VideoCodec.h264, AudioCodec.aac, SubtitleCodec.copy)]
    public async Task BasicTranscode_InputFileWithSubtitles_SkipSubtitlesWithParameter(VideoCodec videoCodec, AudioCodec audioCodec, SubtitleCodec subtitleCodec)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Transcode(Resources.MkvWithSubtitles, output.FullName, videoCodec, audioCodec, subtitleCodec, false)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        mediaInfo.SubtitleStreams.Should().BeEmpty();
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        audioStream.Should().NotBeNull();
        videoStream.Codec.Should().Be(videoCodec.ToStringFast());
        audioStream.Codec.Should().Be(audioCodec.ToStringFast());
        Assert.Equal(25, videoStream.Framerate);
    }

    [Fact]
    public async Task BasicConversion_SloMoVideo_CorrectFramerate()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Convert(Resources.SloMoMp4, output.FullName)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(3, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        videoStream.Codec.Should().Be("h264");
        mediaInfo.SubtitleStreams.Should().BeEmpty();
        // It does not has to be the same
        Assert.Equal(116, (int)videoStream.Framerate);
    }

    [Fact]
    public async Task BasicConversion_InputFileWithMultipleStreams_CorrectResult()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.Convert(Resources.MultipleStream, output.FullName)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(46, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Equal(2, mediaInfo.AudioStreams.Count());
        mediaInfo.SubtitleStreams.Should().BeEmpty();
        var videoStream = mediaInfo.VideoStreams.First();
        videoStream.Should().NotBeNull();
        Assert.Equal(24, videoStream.Framerate);
    }

    [Fact]
    public async Task Rtsp_GotTwoStreams_SaveEverything()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await rtspServer.Publish(Resources.BunnyMp4, "bunny");
        await Task.Delay(2000);

        var mediaInfo = await FFmpeg.GetMediaInfo(rtspServer.Container.GetResourceUri("bunny"));

        await FFmpeg.Conversions.Create().AddStreams(mediaInfo.Streams).SetInputTime(TimeSpan.FromSeconds(3)).SetOutput(output.FullName).Start();

        var result = await FFmpeg.GetMediaInfo(output);
        result.Duration.Should().BeGreaterThan(0.Seconds());
        Assert.Single(result.VideoStreams);
        Assert.Single(result.AudioStreams);
        result.SubtitleStreams.Should().BeEmpty();
        result.VideoStreams.First().Codec.Should().Be("h264");
        Assert.Equal(23, (int)result.VideoStreams.First().Framerate);
        Assert.Equal(640, result.VideoStreams.First().Width);
        Assert.Equal(360, result.VideoStreams.First().Height);
        result.AudioStreams.First().Codec.Should().Be("aac");
    }
}
