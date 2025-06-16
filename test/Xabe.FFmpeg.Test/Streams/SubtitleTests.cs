namespace Xabe.FFmpeg.Test;

public class SubtitleTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(Format.ass, ".ass", "ass")]
    [InlineData(Format.webvtt, ".vtt", "webvtt")]
    [InlineData(Format.srt, ".srt", "subrip")]
    public async Task ConvertTest(Format format, string extension, string expectedFormat)
    {
        var outputPath = storageFixture.CreateMediaLocation().WithExtension(extension);

        var info = await FFmpeg.GetMediaInfo(Resources.SubtitleSrt, _testCancellationToken);

        var subtitleStream = info.SubtitleStreams.FirstOrDefault();
        await FFmpeg.Conversions.Create()
                    .AddStream(subtitleStream)
                    .SetOutput(outputPath)
                    .SetOutputFormat(format)
                    .Start(_testCancellationToken);

        var resultInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Single(resultInfo.SubtitleStreams);
        var resultSteam = resultInfo.SubtitleStreams.First();
        resultSteam.Codec.Should().NotBeNull().And.Subject.ToLower().Should().Be(expectedFormat);
    }

    [Theory]
    [InlineData(".ass", "ass", false)]
    [InlineData(".vtt", "webvtt", false)]
    [InlineData(".srt", "subrip", false)]
    public async Task ExtractSubtitles(string extension, string expectedFormat, bool checkOutputLanguage)
    {
        var outputPath = storageFixture.CreateMediaLocation().WithExtension(extension);
        var info = await FFmpeg.GetMediaInfo(Resources.MultipleStream, _testCancellationToken);

        var subtitleStream = info.SubtitleStreams.FirstOrDefault(x => x.Language == "spa");
        Assert.NotNull(subtitleStream);

        await FFmpeg.Conversions.Create()
                    .AddStream(subtitleStream)
                    .SetOutput(outputPath)
                    .Start(_testCancellationToken);

        var resultInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Empty(resultInfo.VideoStreams);
        Assert.Empty(resultInfo.AudioStreams);
        Assert.Single(resultInfo.SubtitleStreams);
        Assert.Equal(expectedFormat, resultInfo.SubtitleStreams.First().Codec);

        if (checkOutputLanguage)
        {
            Assert.Equal("spa", resultInfo.SubtitleStreams.First().Language);
        }

        resultInfo.SubtitleStreams.First().IsDefault.Should().BeFalse();
        resultInfo.SubtitleStreams.First().IsForced.Should().BeFalse();
    }
}
