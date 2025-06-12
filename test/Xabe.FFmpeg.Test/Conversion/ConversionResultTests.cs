namespace Xabe.FFmpeg.Test;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common.Fixtures;
using Exceptions;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;

public class ConversionResultTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    [Theory]
    [InlineData(null)]
    [InlineData(ProcessPriorityClass.BelowNormal)]
    public async Task ConversionResultTest(ProcessPriorityClass? priority)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var outputPath = storageFixture.GetTempFileName(extension: ".mp4");

        var result = await (await FFmpeg.Conversions.FromSnippet.ToMp4(Resources.FlvWithAudio, outputPath.FullName))
                           .SetPreset(ConversionPreset.UltraFast)
                           .SetPriority(priority)
                           .Start(cancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, cancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Should().NotBeNull();
            result.StartTime.Should().BeAfter(DateTime.MinValue);
            result.EndTime.Should().BeAfter(DateTime.MinValue);
            mediaInfo.Duration.Should().Be(5.Seconds().And(160.Milliseconds()));
            mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        }
    }

    [Fact]
    public async Task ConversionWithWrongInputTest2()
    {
        var randomFileName = storageFixture.GetTempFileName();
        await FluentActions.Awaiting(() => FFmpeg.GetMediaInfo(randomFileName, TestContext.Current.CancellationToken))
                           .Should().ThrowAsync<InvalidInputException>()
                           .WithMessage($"Input file {randomFileName} doesn't exist.");
    }
}
