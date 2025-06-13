namespace Xabe.FFmpeg.Test;

public class SubtitleSnippetsTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task AddSubtitleTest()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
        var input = Resources.MkvWithAudio;
        await FFmpeg.Conversions.FromSnippet.AddSubtitle(input, output, Resources.SubtitleSrt, cancellationToken: _testCancellationToken)
                    .StartConversion(cancellationToken: _testCancellationToken);

        var outputInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(51, outputInfo.Duration.Minutes);
        Assert.Equal(11, outputInfo.Duration.Seconds);
        Assert.Single(outputInfo.SubtitleStreams);
        Assert.Single(outputInfo.VideoStreams);
        Assert.Single(outputInfo.AudioStreams);
    }

    [Fact]
    public async Task AddSubtitleWithLanguageTest()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
        var input = Resources.MkvWithAudio;

        var language = "pol";
        _ = await (await FFmpeg.Conversions.FromSnippet.AddSubtitle(input, output, Resources.SubtitleSrt, language, _testCancellationToken))
                  .SetPreset(ConversionPreset.UltraFast)
                  .Start(_testCancellationToken);

        var outputInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(51, outputInfo.Duration.Minutes);
        Assert.Single(outputInfo.SubtitleStreams);
        Assert.Single(outputInfo.VideoStreams);
        Assert.Single(outputInfo.AudioStreams);
        Assert.Equal(language, outputInfo.SubtitleStreams.First().Language);
    }

    [Theory]
    [InlineData(SubtitleCodec.webvtt)]
    [InlineData(SubtitleCodec.subrip)]
    [InlineData(SubtitleCodec.copy)]
    [InlineData(SubtitleCodec.ass)]
    [InlineData(SubtitleCodec.ssa)]
    public async Task AddSubtitleWithCodecTest(SubtitleCodec subtitleCodec)
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
        var input = Resources.MkvWithAudio;
        await FFmpeg.Conversions.FromSnippet.AddSubtitle(input, output, Resources.SubtitleSrt, subtitleCodec, cancellationToken: _testCancellationToken)
                    .StartConversion(_testCancellationToken);

        var outputInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(51, outputInfo.Duration.Minutes);
        Assert.Single(outputInfo.SubtitleStreams);
        Assert.Single(outputInfo.VideoStreams);
        Assert.Single(outputInfo.AudioStreams);

        if (subtitleCodec.ToString() == "copy")
        {
            Assert.Equal("subrip", outputInfo.SubtitleStreams.First().Codec);
        }
        else if (subtitleCodec.ToString() == "ssa")
        {
            Assert.Equal("ass", outputInfo.SubtitleStreams.First().Codec);
        }
        else
        {
            Assert.Equal(subtitleCodec.ToString(), outputInfo.SubtitleStreams.First().Codec);
        }
    }

    [Theory]
    [InlineData(SubtitleCodec.webvtt)]
    [InlineData(SubtitleCodec.subrip)]
    [InlineData(SubtitleCodec.copy)]
    [InlineData(SubtitleCodec.ass)]
    [InlineData(SubtitleCodec.ssa)]
    public async Task AddSubtitleWithLanguageAndCodecTest(SubtitleCodec subtitleCodec)
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mkv);
        var input = Resources.MkvWithAudio;

        var language = "pol";
        _ = await (await FFmpeg.Conversions.FromSnippet.AddSubtitle(input, output, Resources.SubtitleSrt, subtitleCodec, language, _testCancellationToken))
                  .SetPreset(ConversionPreset.UltraFast)
                  .Start(_testCancellationToken);

        var outputInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(51, outputInfo.Duration.Minutes);
        Assert.Single(outputInfo.SubtitleStreams);
        Assert.Single(outputInfo.VideoStreams);
        Assert.Single(outputInfo.AudioStreams);
        Assert.Equal(language, outputInfo.SubtitleStreams.First().Language);

        if (subtitleCodec.ToString() == "copy")
        {
            Assert.Equal("subrip", outputInfo.SubtitleStreams.First().Codec);
        }
        else if (subtitleCodec.ToString() == "ssa")
        {
            Assert.Equal("ass", outputInfo.SubtitleStreams.First().Codec);
        }
        else
        {
            Assert.Equal(subtitleCodec.ToString(), outputInfo.SubtitleStreams.First().Codec);
        }
    }

    [Fact]
    public async Task BurnSubtitleTest()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
        var input = Resources.Mp4;
        _ = await (await FFmpeg.Conversions.FromSnippet.BurnSubtitle(input, output, Resources.SubtitleSrt, _testCancellationToken))
                  .SetPreset(ConversionPreset.UltraFast)
                  .Start(_testCancellationToken);

        var outputInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(13, outputInfo.Duration.Seconds);
    }

    [Fact]
    public async Task BasicConversion_InputFileWithSubtitles_SkipSubtitles()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithSubtitles, output.FullName, cancellationToken: _testCancellationToken)
                    .StartConversion(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.NotNull(audioStream);
        Assert.Equal("h264", videoStream.Codec);
        Assert.Equal("aac", audioStream.Codec);
        Assert.Empty(mediaInfo.SubtitleStreams);
    }
}
