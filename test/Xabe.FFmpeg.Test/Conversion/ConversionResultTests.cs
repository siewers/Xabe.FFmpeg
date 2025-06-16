namespace Xabe.FFmpeg.Test;

using System.Diagnostics;
using Exceptions;

public class ConversionResultTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(null)]
    [InlineData(ProcessPriorityClass.BelowNormal)]
    public async Task ConversionResultTest(ProcessPriorityClass? priority)
    {
        var outputPath = storageFixture.CreateMediaLocation().WithExtension(FileExtensions.Mp4);

        var conversion = await FFmpeg.Conversions.FromSnippet.ToMp4(Resources.FlvWithAudio, outputPath, _testCancellationToken);

        var result = await conversion.SetPreset(ConversionPreset.UltraFast)
                                     .SetPriority(priority)
                                     .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Should().NotBeNull();
            result.StartTime.Should().BeAfter(DateTime.MinValue);
            result.EndTime.Should().BeAfter(result.StartTime);
            mediaInfo.Duration.Should().Be(5.Seconds().And(160.Milliseconds()));
            mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        }
    }

    [Fact]
    public async Task ConversionWithWrongInputTest2()
    {
        var randomFileName = storageFixture.CreateMediaLocation();
        await FluentActions.Awaiting(() => FFmpeg.GetMediaInfo(randomFileName, _testCancellationToken))
                           .Should().ThrowAsync<InvalidInputException>()
                           .WithMessage($"Input file {randomFileName} doesn't exist.");
    }
}
