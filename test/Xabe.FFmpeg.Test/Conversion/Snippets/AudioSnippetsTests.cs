namespace Xabe.FFmpeg.Test;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Fixtures;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;

public class AudioSnippetsTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    [Fact]
    public async Task AddAudio()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.AddAudio(Resources.Mp4, Resources.Mp3, output))
            .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.Single(mediaInfo.AudioStreams);
        Assert.Equal("aac", mediaInfo.AudioStreams.First()
                                     .Codec
                    );

        Assert.Single(mediaInfo.VideoStreams);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
    }

    [Fact]
    public async Task ExtractAudio()
    {
        var output = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp3);
        var conversion = await FFmpeg.Conversions.FromSnippet.ExtractAudio(Resources.Mp4WithAudio, output);
        await conversion.Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        mediaInfo.VideoStreams.Should().BeEmpty();
        mediaInfo.AudioStreams.Should().ContainSingle()
                 .Which.Should().Satisfy<IAudioStream>(stream =>
                                                       {
                                                           stream.Codec.Should().Be("mp3");
                                                           stream.Duration.Should().Be(13.Seconds().And(536.Milliseconds()));
                                                           stream.Bitrate.Should().Be(320000);
                                                       }
                                                      );
    }

    [Theory]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.bar, AmplitudeScale.lin, FrequencyScale.log)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.bar, AmplitudeScale.log, FrequencyScale.lin)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.bar, AmplitudeScale.sqrt, FrequencyScale.rlog)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.bar, AmplitudeScale.cbrt, FrequencyScale.log)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.dot, AmplitudeScale.lin, FrequencyScale.log)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.dot, AmplitudeScale.log, FrequencyScale.lin)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.dot, AmplitudeScale.sqrt, FrequencyScale.rlog)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.dot, AmplitudeScale.cbrt, FrequencyScale.log)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.line, AmplitudeScale.lin, FrequencyScale.log)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.line, AmplitudeScale.log, FrequencyScale.lin)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.line, AmplitudeScale.sqrt, FrequencyScale.rlog)]
    [InlineData(VideoSize.Hd1080, PixelFormat.yuv420p, VisualisationMode.line, AmplitudeScale.cbrt, FrequencyScale.log)]
    public async Task VisualiseAudioTest(VideoSize size, PixelFormat pixelFormat, VisualisationMode mode, AmplitudeScale amplitudeScale, FrequencyScale frequencyScale)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.aac);
        _ = await (await FFmpeg.Conversions.FromSnippet.VisualizeAudio(Resources.Mp4WithAudio, output.FullName, size, pixelFormat, mode, amplitudeScale, frequencyScale))
            .Start();

        var resultFile = await FFmpeg.GetMediaInfo(output);

        // The resulting streams are 4 seconds longer than the original
        Assert.Equal((audioStream.Duration + TimeSpan.FromSeconds(4)).Seconds, resultFile.VideoStreams.First().Duration.Seconds);
        Assert.Equal((audioStream.Duration + TimeSpan.FromSeconds(4)).Seconds, resultFile.AudioStreams.First().Duration.Seconds);
        Assert.Equal(1920, resultFile.VideoStreams.First().Width);
        Assert.Equal(1080, resultFile.VideoStreams.First().Height);
    }
}
