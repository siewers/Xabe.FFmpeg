namespace Xabe.FFmpeg.Test;

using System.Linq;
using System.Threading.Tasks;
using Common.Fixtures;
using Xunit;

public class SubtitleTests : IClassFixture<StorageFixture>
{
    private readonly StorageFixture _storageFixture;

    public SubtitleTests(StorageFixture storageFixture)
    {
        _storageFixture = storageFixture;
    }

    [Theory]
    [InlineData(Format.ass, ".ass", "ass")]
    [InlineData(Format.webvtt, ".vtt", "webvtt")]
    [InlineData(Format.srt, ".srt", "subrip")]
    public async Task ConvertTest(Format format, string extension, string expectedFormat)
    {
        var outputPath = _storageFixture.GetTempFileName(extension);

        var info = await FFmpeg.GetMediaInfo(Resources.SubtitleSrt);

        var subtitleStream = info.SubtitleStreams.FirstOrDefault();
        _ = await FFmpeg.Conversions.New()
                        .AddStream(subtitleStream)
                        .SetOutput(outputPath)
                        .SetOutputFormat(format)
                        .Start();

        var resultInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Single(resultInfo.SubtitleStreams);
        var resultSteam = resultInfo.SubtitleStreams.First();
        Assert.Equal(expectedFormat, resultSteam.Codec.ToLower());
    }

    [Theory]
    [InlineData(".ass", "ass", false)]
    [InlineData(".vtt", "webvtt", false)]
    [InlineData(".srt", "subrip", false)]
    public async Task ExtractSubtitles(string extension, string expectedFormat, bool checkOutputLanguage)
    {
        var outputPath = _storageFixture.GetTempFileName(extension);
        var info = await FFmpeg.GetMediaInfo(Resources.MultipleStream);

        var subtitleStream = info.SubtitleStreams.FirstOrDefault(x => x.Language == "spa");
        Assert.NotNull(subtitleStream);

        var result = await FFmpeg.Conversions.New()
                                 .AddStream(subtitleStream)
                                 .SetOutput(outputPath)
                                 .Start();

        var resultInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Empty(resultInfo.VideoStreams);
        Assert.Empty(resultInfo.AudioStreams);
        Assert.Single(resultInfo.SubtitleStreams);
        Assert.Equal(expectedFormat, resultInfo.SubtitleStreams.First().Codec);

        if (checkOutputLanguage)
        {
            Assert.Equal("spa", resultInfo.SubtitleStreams.First().Language);
        }

        Assert.False(resultInfo.SubtitleStreams.First().IsDefault.Value);
        Assert.False(resultInfo.SubtitleStreams.First().IsForced.Value);
    }
}
