namespace Xabe.FFmpeg.Test;

using System.Globalization;
using Exceptions;

public class AudioStreamTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(13, 13, 1.0)]
    [InlineData(6, 6, 2.0)]
    [InlineData(27, 27, 0.5)]
    public async Task ChangeSpeedTest(int expectedDuration, int expectedAudioDuration, double speed)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp3, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp3);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.AudioStreams.First().ChangeSpeed(speed))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(expectedDuration, mediaInfo.Duration.Seconds);
        Assert.Equal(expectedAudioDuration, mediaInfo.AudioStreams.First().Duration.Seconds);
        mediaInfo.AudioStreams.First().Codec.Should().Be("mp3");
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Theory]
    [InlineData(192000)]
    [InlineData(32000)]
    public async Task SetBitrate(int expectedBitrate)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp3, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp3);

        var audioStream = inputFile.AudioStreams.First();
        audioStream.SetBitrate(expectedBitrate);
        await FFmpeg.Conversions.Create()
                    .AddStream(audioStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);

        Assert.Equal(expectedBitrate, mediaInfo.AudioStreams.First().Bitrate);
        mediaInfo.AudioStreams.First().Codec.Should().Be("mp3");
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task SetBitrate_WithMaximumBitrate()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp3, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp3);

        var audioStream = inputFile.AudioStreams.First();
        audioStream.SetBitrate(32000, 32000, 8000);
        await FFmpeg.Conversions.Create()
                    .AddStream(audioStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);

        Assert.Equal(32000, mediaInfo.AudioStreams.First().Bitrate);
        mediaInfo.AudioStreams.First().Codec.Should().Be("mp3");
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task ChangeChannels()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp3, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp3);

        var audioStream = inputFile.AudioStreams.First();
        var channels = audioStream.Channels;
        Assert.Equal(2, channels);
        audioStream.SetChannels(1);
        await FFmpeg.Conversions.Create()
                    .AddStream(audioStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);

        Assert.Equal(1, mediaInfo.AudioStreams.First().Channels);
        mediaInfo.AudioStreams.First().Codec.Should().Be("mp3");
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task ChangeSamplerate()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp3, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp3);

        var audioStream = inputFile.AudioStreams.First();
        var sampleRate = audioStream.SampleRate;
        Assert.Equal(48000, sampleRate);
        audioStream.SetSampleRate(44100);
        await FFmpeg.Conversions.Create()
                    .AddStream(audioStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);

        Assert.Equal(44100, mediaInfo.AudioStreams.First().SampleRate);
        mediaInfo.AudioStreams.First().Codec.Should().Be("mp3");
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task OnConversion_ExtractOnlyAudioStream_OnProgressFires()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var conversion = FFmpeg.Conversions.Create()
                               .AddStream(inputFile.AudioStreams.First()
                                                   .SetSeek(TimeSpan.FromSeconds(2))
                                         )
                               .SetOutput(outputPath.FullName);

        var currentProgress = TimeSpan.Zero;
        var videoLength = TimeSpan.Zero;
        conversion.OnProgress += (_, e) =>
                                 {
                                     currentProgress = e.Duration;
                                     videoLength = e.TotalLength;
                                 };

        await conversion.Start(_testCancellationToken);

        currentProgress.Should().BeGreaterThan(TimeSpan.Zero);
        currentProgress.Should().BeLessThanOrEqualTo(videoLength);
        videoLength.TotalSeconds.Should().Be(7);
    }

    [Fact]
    public async Task ExtractAdditionalValuesTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        inputFile.AudioStreams.First().IsDefault.Should().BeTrue();
        inputFile.AudioStreams.First().IsForced.Should().BeFalse();
        inputFile.AudioStreams.First().Language.Should().Be("und");
    }

    [Fact]
    public async Task ChangeSpeed_CommaAsASeparator_CorrectResult()
    {
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("pl-PL");

        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp3, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp3);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.AudioStreams.First().ChangeSpeed(0.5))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(27, mediaInfo.Duration.Seconds);
        Assert.Equal(27, mediaInfo.AudioStreams.First().Duration.Seconds);
        Assert.Equal("mp3", mediaInfo.AudioStreams.First().Codec);
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task SetBitstreamFilter_CorrectInput_CorrectResult()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.AudioStreams.First().SetBitstreamFilter(BitstreamFilter.aac_adtstoasc))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal(9, mediaInfo.AudioStreams.First().Duration.Seconds);
        Assert.Equal("aac", mediaInfo.AudioStreams.First().Codec);
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task SetBitstreamFilter_IncorrectFilter_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(inputFile.AudioStreams.First().SetBitstreamFilter(BitstreamFilter.h264_mp4toannexb))
                                                                            .SetOutput(outputPath.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConversionExceptionBase>();
        exception.InnerException.Should().BeOfType<InvalidBitstreamFilterException>();
    }

    [Fact]
    public async Task SetBitstreamFilter_CorrectInputAsString_CorrectResult()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.AudioStreams.First().SetBitstreamFilter("aac_adtstoasc"))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal(9, mediaInfo.AudioStreams.First().Duration.Seconds);
        Assert.Equal("aac", mediaInfo.AudioStreams.First().Codec);
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task SetBitstreamFilter_IncorrectFilterAsString_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(inputFile.AudioStreams.First().SetBitstreamFilter("h264_mp4toannexb"))
                                                                            .SetOutput(outputPath.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConversionExceptionBase>();
        exception.InnerException.Should().BeOfType<InvalidBitstreamFilterException>();
    }

    [Theory]
    [InlineData(AudioCodec.mp2, "mp2")]
    [InlineData(AudioCodec._4gv, "4gv")]
    [InlineData(AudioCodec._8svx_exp, "8svx_exp")]
    [InlineData(AudioCodec._8svx_fib, "8svx_fib")]
    public async Task ChangeCodec_EnumValue_EverythingMapsCorrectly(AudioCodec audioCodec, string expectedCodec)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4WithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var audioStream = inputFile.AudioStreams.First();
        audioStream.SetCodec(audioCodec);

        var args = FFmpeg.Conversions.Create()
                         .AddStream(audioStream)
                         .SetOutput(outputPath.FullName)
                         .Build();

        Assert.Contains($"-c:a {expectedCodec}", args);
    }

    [Fact]
    public async Task ChangeCodec_StringValue_CorrectResult()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4WithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var audioStream = inputFile.AudioStreams.First();
        audioStream.SetCodec("mp3");
        await FFmpeg.Conversions.Create()
                    .AddStream(audioStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Equal(13, mediaInfo.AudioStreams.First().Duration.Seconds);
        Assert.Equal("mp3", mediaInfo.AudioStreams.First().Codec);
        Assert.NotEmpty(mediaInfo.AudioStreams);
    }

    [Fact]
    public async Task ChangeCodec_IncorrectCodec_NotFound()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4WithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var audioStream = inputFile.AudioStreams.First();
        audioStream.SetCodec("notExisting");

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(audioStream)
                                                                            .SetOutput(outputPath.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConversionExceptionBase>();
    }

    [Fact]
    public async Task CopyStream_CorrectFFmpegArguments()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4WithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var audioStream = inputFile.AudioStreams.First();
        audioStream.SetCodec(AudioCodec.comfortnoise);
        audioStream.CopyStream();

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(audioStream)
                                           .SetOutput(outputPath.FullName)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Should().Be($" -i \"{Path.GetFullPath(inputFile.Location.AbsolutePath)}\" -c:a copy -map 0:1 -n   \"{outputPath.FullName}\"");
    }

    [Fact]
    public async Task SetInputFormat_ChangeIfFormatIsApplied()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp3, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp3);

        var audioStream = inputFile.AudioStreams.First();
        audioStream.SetInputFormat(Format.mp3);

        var result = await FFmpeg.Conversions.Create()
                                 .AddStream(audioStream)
                                 .SetOutput(outputPath.FullName)
                                 .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);

        result.Arguments.Should().Contain("-f mp3 -i");
        mediaInfo.AudioStreams.Should().NotBeEmpty();
    }
}
