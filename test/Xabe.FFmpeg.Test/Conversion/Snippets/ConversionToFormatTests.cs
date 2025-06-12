using Xabe.FFmpeg.Test.Common.Fixtures;

namespace Xabe.FFmpeg.Test;

public class ConversionToFormatTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    [Theory]
    [InlineData(1, 0, 25)]
    [InlineData(1, 1, 24.889)]
    public async Task ToGifTest(int loopCount, int delay, double framerate)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Gif);
        _ = await (await FFmpeg.Conversions.FromSnippet.ToGif(Resources.Mp4, output.FullName, loopCount, delay))
                  .SetPreset(ConversionPreset.UltraFast)
                  .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Empty(mediaInfo.AudioStreams);
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.Equal("gif", videoStream.Codec);
        Assert.Equal("16:9", videoStream.Ratio);
        Assert.Equal(framerate, videoStream.Framerate);
        Assert.Equal(1280, videoStream.Width);
        Assert.Equal(720, videoStream.Height);
    }

    public static IEnumerable<object[]> JoinFiles => new[]
                                                     {
                                                         new object[] {Resources.MkvWithAudio, Resources.Mp4WithAudio, 23, 1280, 720, "16:9"},
                                                         new object[] {Resources.MkvWithAudio, Resources.MkvWithAudio, 19, 320, 240, "4:3"},
                                                         new object[] {Resources.MkvWithAudio, Resources.Mp4, 23, 1280, 720, "16:9" },
                                                     };

    [Fact]
    public async Task ToMp4Test()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.ToMp4(Resources.MkvWithAudio, output.FullName))
            .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.NotNull(audioStream);
        Assert.Equal("h264", videoStream.Codec);
        Assert.Equal("aac", audioStream.Codec);
    }

    [Fact]
    public async Task ToOgvTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ogv);
        _ = await (await FFmpeg.Conversions.FromSnippet.ToOgv(Resources.MkvWithAudio, output.FullName))
            .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.NotNull(audioStream);
        Assert.Equal("theora", videoStream.Codec);
        Assert.Equal("vorbis", audioStream.Codec);
    }

    [Fact]
    public async Task ToTsTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        _ = await (await FFmpeg.Conversions.FromSnippet.ToTs(Resources.Mp4WithAudio, output.FullName))
            .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.NotNull(audioStream);
        Assert.Equal("mpeg2video", videoStream.Codec);
        Assert.Equal("mp2", audioStream.Codec);
    }

    [Fact]
    public async Task ToWebMTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.WebM);
        _ = await (await FFmpeg.Conversions.FromSnippet.ToWebM(Resources.Mp4WithAudio, output.FullName))
                  .SetPreset(ConversionPreset.UltraFast)
                  .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.NotNull(audioStream);
        Assert.Equal("vp8", videoStream.Codec);
        Assert.Equal("vorbis", audioStream.Codec);
    }

    [Fact]
    public async Task ConversionWithoutSpecificFormat()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await Conversion.ConvertAsync(Resources.MkvWithAudio, output.FullName)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.NotNull(audioStream);
        Assert.Equal("h264", videoStream.Codec);
        Assert.Equal("aac", audioStream.Codec);
    }
}
