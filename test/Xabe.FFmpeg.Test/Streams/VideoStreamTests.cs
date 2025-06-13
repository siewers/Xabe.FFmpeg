namespace Xabe.FFmpeg.Test;

using System.Globalization;
using Exceptions;

public class VideoStreamTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(RotateDegrees.Clockwise)]
    [InlineData(RotateDegrees.Invert)]
    public async Task TransposeTest(RotateDegrees rotateDegrees)
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First().Rotate(rotateDegrees))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task Pad()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.Pad(480, 640);

        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(640, mediaInfo.VideoStreams.First().Height);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task ChangeFramerate()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        var originalFramerate = videoStream.Framerate;
        Assert.Equal(25, originalFramerate);
        videoStream.SetFramerate(24);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(24, mediaInfo.VideoStreams.First().Framerate);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SetBitrate()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.Bitrate.Should().Be(860233);

        videoStream.SetBitrate(860237);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        mediaInfo.VideoStreams.First().Bitrate.Should().BeInRange(560000, 580000);
        mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        mediaInfo.AudioStreams.Should().BeEmpty();
    }

    [Fact]
    public async Task SetBitrate_WithMaxBitrateAndBuffer()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        var originalBitrate = videoStream.Bitrate;
        originalBitrate.Should().Be(860233);

        videoStream.SetBitrate(6000, 6000, 6000);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        mediaInfo.VideoStreams.Should().ContainSingle()
                 .Which.Should().Satisfy<IVideoStream>(stream =>
                                                       {
                                                           stream.Bitrate.Should().BeInRange(7000, 8000);
                                                           stream.Codec.Should().Be("h264");
                                                       }
                                                      );

        mediaInfo.AudioStreams.Should().BeEmpty();
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags("ilme", "ildct");

        var result = await FFmpeg.Conversions.Create()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath.FullName)
                                 .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains("-flags +ilme+ildct", result.Arguments);
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags_FlagsWithPlus_CorrectConversion()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags("ilme", "ildct");

        var result = await FFmpeg.Conversions.Create()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath.FullName)
                                 .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains("-flags +ilme+ildct", result.Arguments);
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags_UseString_CorrectConversion()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags(Flag.ilme, Flag.ildct);

        var result = await FFmpeg.Conversions.Create()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath.FullName)
                                 .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains("-flags +ilme+ildct", result.Arguments);
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags_ConcatenatedFlags_CorrectConversion()
    {
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags("+ilme+ildct");

        var result = await FFmpeg.Conversions.Create()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath.FullName)
                                 .Start(_testCancellationToken);

        result.Arguments.Should().Contain("-flags +ilme+ildct");

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        mediaInfo.AudioStreams.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1.0, 9)]
    [InlineData(2.0, 5)]
    [InlineData(0.5, 19)]
    public async Task ChangeSpeedTest(double speed, int expectedVideoDuration)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First().SetCodec(VideoCodec.h264).ChangeSpeed(speed))
                    .SetPreset(ConversionPreset.UltraFast)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        mediaInfo.AudioStreams.Should().BeEmpty();
        mediaInfo.VideoStreams.First().Should().Satisfy<IVideoStream>(stream =>
                                                                      {
                                                                          stream.Duration.Should().BeCloseTo(expectedVideoDuration.Seconds(), 1.Seconds());
                                                                          stream.Codec.Should().Be("h264");
                                                                      }
                                                                     );
    }

    [Theory]
    [InlineData(0.4)]
    [InlineData(2.5)]
    public async Task ChangeMediaSpeedSTestArgumentOutOfRange(double multiplier)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await FFmpeg.Conversions.Create()
                                                                                      .AddStream(inputFile.VideoStreams.First()
                                                                                                          .SetCodec(VideoCodec.h264)
                                                                                                          .ChangeSpeed(multiplier)
                                                                                                )
                                                                                      .SetPreset(ConversionPreset.UltraFast)
                                                                                      .SetOutput(outputPath.FullName)
                                                                                      .Start(_testCancellationToken)
                                                             );
    }

    [Fact]
    public async Task BurnSubtitlesTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First().AddSubtitles(Resources.SubtitleSrt))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        mediaInfo.Duration.Should().Be(9.Seconds().And(880.Milliseconds()));
        mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        mediaInfo.AudioStreams.Should().BeEmpty();
    }

    [Fact]
    public async Task BurnSubtitlesWithParametersTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var conversion = FFmpeg.Conversions.Create()
                               .AddStream(inputFile.VideoStreams.First()
                                                   .AddSubtitles(Resources.SubtitleSrt, VideoSize.Xga, "UTF-8", "Fontsize=20,PrimaryColour=&H00ffff&,MarginV=30")
                                         )
                               .SetOutput(outputPath.FullName);

        await conversion.Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains(":charenc=UTF-8:force_style='Fontsize=20,PrimaryColour=&H00ffff&,MarginV=30':original_size=1024x768", conversion.Build());
    }

    [Fact]
    public async Task ChangeOutputFramesCountTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First()
                                        .SetOutputFramesCount(50)
                              )
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(TimeSpan.FromSeconds(2), mediaInfo.Duration);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Equal(50, mediaInfo.Duration.TotalSeconds * mediaInfo.VideoStreams.First().Framerate);
    }

    [Fact]
    public async Task IncompatibleParametersTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var act = async () =>
                  {
                      await FFmpeg.Conversions.Create()
                                  .AddStream(inputFile.VideoStreams.First()
                                                      .SetCodec(VideoCodec.h264)
                                                      .Reverse()
                                                      .CopyStream()
                                            )
                                  .SetOutput(outputPath.FullName)
                                  .Start(_testCancellationToken);
                  };

        (await act.Should().ThrowExactlyAsync<ConversionExceptionBase>())
            .Which.Should().Satisfy<ConversionExceptionBase>(ex =>
                                                             {
                                                                 ex.InputParameters.Should().ContainAll("-c:v copy", "-vf reverse");
                                                                 ex.Message.Should().Contain("Filtergraph 'reverse' was specified, but codec copy was selected. Filtering and streamcopy cannot be used together.");
                                                             }
                                                            );
    }

    [Fact]
    public async Task LoopTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Gif);
        _ = await FFmpeg.Conversions.Create()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetLoop(1)
                                  )
                        .SetOutput(outputPath.FullName)
                        .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);

        mediaInfo.Duration.Should().Be(13.Seconds().And(480.Milliseconds()));
        mediaInfo.AudioStreams.Should().BeEmpty();
        mediaInfo.VideoStreams.Should().ContainSingle()
                 .Which.Should().Satisfy<IVideoStream>(stream =>
                                                       {
                                                           stream.Codec.Should().Be("gif");
                                                           stream.Ratio.Should().Be("16:9");
                                                           stream.Framerate.Should().Be(25);
                                                           stream.Width.Should().Be(1280);
                                                           stream.Height.Should().Be(720);
                                                       }
                                                      );
    }

    [Fact]
    public async Task ReverseTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First()
                                        .SetCodec(VideoCodec.h264)
                                        .Reverse()
                              )
                    .SetPreset(ConversionPreset.UltraFast)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SeekLengthTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var conversion = FFmpeg.Conversions.Create()
                               .AddStream(inputFile.VideoStreams.First())
                               .SetOutput(outputPath.FullName)
                               .SetSeek(TimeSpan.FromSeconds(2));

        var currentProgress = TimeSpan.Zero;
        var videoLength = TimeSpan.Zero;
        conversion.OnProgress += (_, e) =>
                                 {
                                     currentProgress = e.Duration;
                                     videoLength = e.TotalLength;
                                 };

        await conversion.Start(_testCancellationToken);

        Assert.True(currentProgress > TimeSpan.Zero);
        Assert.True(currentProgress <= videoLength);
        Assert.True(videoLength == TimeSpan.FromSeconds(7));
    }

    [Fact]
    public async Task SimpleConversionTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First())
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task X265Test()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First()
                                        .SetCodec(VideoCodec.hevc)
                              )
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("hevc", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SizeTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First()
                                        .SetSize(640, 480)
                              )
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Equal(640, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Height);
    }

    [Fact]
    public async Task SetSize_UseEnum_ResultsAreCorrect()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First()
                                        .SetSize(VideoSize.Sntsc)
                              )
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Equal(640, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Height);
    }

    [Fact]
    public async Task VideoCodecTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Ts);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First()
                                        .SetCodec(VideoCodec.mpeg2video)
                              )
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("mpeg2video", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task ExtractAdditionalValuesTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        inputFile.VideoStreams.First().IsDefault.Should().BeTrue();
        inputFile.VideoStreams.First().IsForced.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeSpeed_CommaAsASeparator_CorrectResult()
    {
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("pl-PL");

        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First().SetCodec(VideoCodec.h264).ChangeSpeed(0.5))
                    .SetPreset(ConversionPreset.UltraFast)
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(19, mediaInfo.Duration.Seconds);
        Assert.Equal(19, mediaInfo.VideoStreams.First().Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SetBitstreamFilter_CorrectInput_CorrectResult()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter(BitstreamFilter.h264_mp4toannexb))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Equal(13, mediaInfo.VideoStreams.First().Duration.Seconds);
        Assert.NotEmpty(mediaInfo.VideoStreams);
    }

    [Fact]
    public async Task SetBitstreamFilter_IncorrectFilter_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter(BitstreamFilter.aac_adtstoasc))
                                                                            .SetOutput(outputPath.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().BeOfType<InvalidBitstreamFilterException>();
    }

    [Fact]
    public async Task SetBitstreamFilter_CorrectInputAsString_CorrectResult()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter("h264_mp4toannexb"))
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Equal(13, mediaInfo.VideoStreams.First().Duration.Seconds);
        Assert.NotEmpty(mediaInfo.VideoStreams);
    }

    [Fact]
    public async Task SetBitstreamFilter_IncorrectFilterAsString_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter("aac_adtstoasc"))
                                                                            .SetOutput(outputPath.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().BeOfType<InvalidBitstreamFilterException>();
    }

    [Theory]
    [InlineData(VideoCodec._4xm, "4xm")]
    [InlineData(VideoCodec._8bps, "8bps")]
    [InlineData(VideoCodec._012v, "012v")]
    public async Task SetCodec_SpecialNames_EverythingIsCorrect(VideoCodec videoCodec, string expectedCodec)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var args = FFmpeg.Conversions.Create()
                         .AddStream(inputFile.VideoStreams.First()
                                             .SetCodec(videoCodec)
                                   )
                         .SetOutput(outputPath.FullName)
                         .Build();

        Assert.Contains($"-c:v {expectedCodec}", args);
    }

    [Fact]
    public async Task SetCodec_InvalidCodec_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(inputFile.VideoStreams.First().SetCodec("notExisting"))
                                                                            .SetOutput(outputPath.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        Assert.NotNull(exception);
        Assert.IsType<ConversionExceptionBase>(exception);
    }

    [Fact]
    public async Task SetSize_ParameterIsOverridden_NewValueIsSet()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddStream(inputFile.VideoStreams.First()
                                        .SetSize(1920, 1080)
                                        .SetSize(640, 480)
                              )
                    .SetOutput(outputPath.FullName)
                    .Start(_testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        Assert.Equal(640, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Height);
    }
}
