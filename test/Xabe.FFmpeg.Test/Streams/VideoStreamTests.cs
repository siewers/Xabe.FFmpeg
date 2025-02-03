namespace Xabe.FFmpeg.Test;

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Fixtures;
using Exceptions;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;

public class VideoStreamTests : IClassFixture<StorageFixture>
{
    private readonly StorageFixture _storageFixture;

    public VideoStreamTests(StorageFixture storageFixture)
    {
        _storageFixture = storageFixture;
    }

    [Theory]
    [InlineData(RotateDegrees.Clockwise)]
    [InlineData(RotateDegrees.Invert)]
    public async Task TransposeTest(RotateDegrees rotateDegrees)
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First().Rotate(rotateDegrees))
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task Pad()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.Pad(480, 640);

        _ = await FFmpeg.Conversions.New()
                        .AddStream(videoStream)
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(640, mediaInfo.VideoStreams.First().Height);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task ChangeFramerate()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        var originalFramerate = videoStream.Framerate;
        Assert.Equal(25, originalFramerate);
        videoStream.SetFramerate(24);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(videoStream)
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(24, mediaInfo.VideoStreams.First().Framerate);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SetBitrate()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        var originalBitrate = videoStream.Bitrate;
        Assert.Equal(860233, originalBitrate);
        videoStream.SetBitrate(860237);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(videoStream)
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.InRange(mediaInfo.VideoStreams.First().Bitrate, 560000, 580000);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SetBitrate_WithMaxBitrateAndBuffer()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        var originalBitrate = videoStream.Bitrate;
        originalBitrate.Should().Be(860233);

        videoStream.SetBitrate(6000, 6000, 6000);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(videoStream)
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        mediaInfo.AudioStreams.Should().ContainSingle();
        mediaInfo.VideoStreams.Should().ContainSingle()
                 .Which.Should().Satisfy<IVideoStream>(stream =>
                                                       {
                                                           stream.Bitrate.Should().BeInRange(7000, 8000);
                                                           stream.Codec.Should().Be("h264");
                                                       }
                                                      );
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags("ilme", "ildct");

        var result = await FFmpeg.Conversions.New()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath)
                                 .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains("-flags +ilme+ildct", result.Arguments);
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags_FlagsWithPlus_CorrectConversion()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags("ilme", "ildct");

        var result = await FFmpeg.Conversions.New()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath)
                                 .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains("-flags +ilme+ildct", result.Arguments);
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags_UseString_CorrectConversion()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags(Flag.ilme, Flag.ildct);

        var result = await FFmpeg.Conversions.New()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath)
                                 .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains("-flags +ilme+ildct", result.Arguments);
    }

    // Check if Filter Flags do work. FFProbe does not support checking for Interlaced or Progressive,
    //  so there is no "real check" here
    [Fact]
    public async Task SetFlags_ContatenatedFlags_CorrectConversion()
    {
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        var videoStream = inputFile.VideoStreams.First();
        videoStream.SetFlags("+ilme+ildct");

        var result = await FFmpeg.Conversions.New()
                                 .AddStream(videoStream)
                                 .SetOutput(outputPath)
                                 .Start();

        result.Arguments.Should().Contain("-flags +ilme+ildct");

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        mediaInfo.AudioStreams.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1.0, 9)]
    [InlineData(2.0, 5)]
    [InlineData(0.5, 19)]
    public async Task ChangeSpeedTest(double speed, int expectedVideoDuration)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First().SetCodec(VideoCodec.h264).ChangeSpeed(speed))
                        .SetPreset(ConversionPreset.UltraFast)
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
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
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await FFmpeg.Conversions.New()
                                                                                      .AddStream(inputFile.VideoStreams.First()
                                                                                                          .SetCodec(VideoCodec.h264)
                                                                                                          .ChangeSpeed(multiplier)
                                                                                                )
                                                                                      .SetPreset(ConversionPreset.UltraFast)
                                                                                      .SetOutput(outputPath)
                                                                                      .Start()
                                                             );
    }

    [Fact]
    public async Task BurnSubtitlesTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        var conversionResult = await FFmpeg.Conversions.New()
                                           .AddStream(inputFile.VideoStreams.First().AddSubtitles(Resources.SubtitleSrt))
                                           .SetOutput(outputPath)
                                           .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        mediaInfo.Duration.Should().Be(9.Seconds().And(880.Milliseconds()));
        mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        mediaInfo.AudioStreams.Should().BeEmpty();
    }

    [Fact]
    public async Task BurnSubtitlesWithParametersTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var conversion = FFmpeg.Conversions.New()
                               .AddStream(inputFile.VideoStreams.First()
                                                   .AddSubtitles(Resources.SubtitleSrt, VideoSize.Xga, "UTF-8", "Fontsize=20,PrimaryColour=&H00ffff&,MarginV=30")
                                         )
                               .SetOutput(outputPath);

        _ = await conversion.Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Contains(":charenc=UTF-8:force_style='Fontsize=20,PrimaryColour=&H00ffff&,MarginV=30':original_size=1024x768", conversion.Build());
    }

    [Fact]
    public async Task ChangeOutputFramesCountTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetOutputFramesCount(50)
                                  )
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(TimeSpan.FromSeconds(2), mediaInfo.Duration);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Equal(50, mediaInfo.Duration.TotalSeconds * mediaInfo.VideoStreams.First().Framerate);
    }

    [Fact]
    public async Task IncompatibleParametersTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var act = async () =>
                  {
                      await FFmpeg.Conversions.New()
                                  .AddStream(inputFile.VideoStreams.First()
                                                      .SetCodec(VideoCodec.h264)
                                                      .Reverse()
                                                      .CopyStream()
                                            )
                                  .SetOutput(outputPath)
                                  .Start();
                  };

        (await act.Should().ThrowExactlyAsync<ConversionException>())
            .Which.Should().Satisfy<ConversionException>(ex =>
                                                         {
                                                             ex.InputParameters.Should().ContainAll("-c:v copy", "-vf reverse");
                                                             ex.Message.Should().Contain("Filtergraph 'reverse' was specified, but codec copy was selected. Filtering and streamcopy cannot be used together.");
                                                         }
                                                        );
    }

    [Fact]
    public async Task LoopTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Gif);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetLoop(1)
                                  )
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);

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
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetCodec(VideoCodec.h264)
                                            .Reverse()
                                  )
                        .SetPreset(ConversionPreset.UltraFast)
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SeekLengthTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var conversion = FFmpeg.Conversions.New()
                               .AddStream(inputFile.VideoStreams.First())
                               .SetOutput(outputPath)
                               .SetSeek(TimeSpan.FromSeconds(2));

        var currentProgress = new TimeSpan();
        var videoLength = new TimeSpan();
        conversion.OnProgress += (sender, e) =>
                                 {
                                     currentProgress = e.Duration;
                                     videoLength = e.TotalLength;
                                 };

        await conversion.Start();

        Assert.True(currentProgress > TimeSpan.Zero);
        Assert.True(currentProgress <= videoLength);
        Assert.True(videoLength == TimeSpan.FromSeconds(7));
    }

    [Fact]
    public async Task SimpleConversionTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First())
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task X265Test()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetCodec(VideoCodec.hevc)
                                  )
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("hevc", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SizeTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetSize(640, 480)
                                  )
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Equal(640, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Height);
    }

    [Fact]
    public async Task SetSize_UseEnum_ResultsAreCorrect()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetSize(VideoSize.Sntsc)
                                  )
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
        Assert.Equal(640, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Height);
    }

    [Fact]
    public async Task VideoCodecTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Ts);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetCodec(VideoCodec.mpeg2video)
                                  )
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(9, mediaInfo.Duration.Seconds);
        Assert.Equal("mpeg2video", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task ExtractAdditionalValuesTest()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);

        Assert.True(inputFile.VideoStreams.First().IsDefault.Value);
        Assert.False(inputFile.VideoStreams.First().IsForced.Value);
    }

    [Fact]
    public async Task ChangeSpeed_CommaAsASeparator_CorrectResult()
    {
        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("pl-PL");

        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First().SetCodec(VideoCodec.h264).ChangeSpeed(0.5))
                        .SetPreset(ConversionPreset.UltraFast)
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(19, mediaInfo.Duration.Seconds);
        Assert.Equal(19, mediaInfo.VideoStreams.First().Duration.Seconds);
        Assert.Equal("h264", mediaInfo.VideoStreams.First().Codec);
        Assert.False(mediaInfo.AudioStreams.Any());
    }

    [Fact]
    public async Task SetBitstreamFilter_CorrectInput_CorrectResult()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter(BitstreamFilter.h264_mp4toannexb))
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Equal(13, mediaInfo.VideoStreams.First().Duration.Seconds);
        Assert.NotEmpty(mediaInfo.VideoStreams);
    }

    [Fact]
    public async Task SetBitstreamFilter_IncorrectFilter_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.New()
                                                                            .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter(BitstreamFilter.aac_adtstoasc))
                                                                            .SetOutput(outputPath)
                                                                            .Start()
                                                   );

        Assert.NotNull(exception);
        Assert.IsType<ConversionException>(exception);
        Assert.IsType<InvalidBitstreamFilterException>(exception.InnerException);
    }

    [Fact]
    public async Task SetBitstreamFilter_CorrectInputAsString_CorrectResult()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter("h264_mp4toannexb"))
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(13, mediaInfo.Duration.Seconds);
        Assert.Equal(13, mediaInfo.VideoStreams.First().Duration.Seconds);
        Assert.NotEmpty(mediaInfo.VideoStreams);
    }

    [Fact]
    public async Task SetBitstreamFilter_IncorrectFilterAsString_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.Mp4);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.New()
                                                                            .AddStream(inputFile.VideoStreams.First().SetBitstreamFilter("aac_adtstoasc"))
                                                                            .SetOutput(outputPath)
                                                                            .Start()
                                                   );

        Assert.NotNull(exception);
        Assert.IsType<ConversionException>(exception);
        Assert.IsType<InvalidBitstreamFilterException>(exception.InnerException);
    }

    [Theory]
    [InlineData(VideoCodec._4xm, "4xm")]
    [InlineData(VideoCodec._8bps, "8bps")]
    [InlineData(VideoCodec._012v, "012v")]
    public async Task SetCodec_SpecialNames_EverythingIsCorrect(VideoCodec videoCodec, string expectedCodec)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var args = FFmpeg.Conversions.New()
                         .AddStream(inputFile.VideoStreams.First()
                                             .SetCodec(videoCodec)
                                   )
                         .SetOutput(outputPath)
                         .Build();

        Assert.Contains($"-c:v {expectedCodec}", args);
    }

    [Fact]
    public async Task SetCodec_InvalidCodec_ThrowConversionException()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.New()
                                                                            .AddStream(inputFile.VideoStreams.First().SetCodec("notExisting"))
                                                                            .SetOutput(outputPath)
                                                                            .Start()
                                                   );

        Assert.NotNull(exception);
        Assert.IsType<ConversionException>(exception);
    }

    [Fact]
    public async Task SetSize_ParameterIsOverrided_NewValueIsSet()
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio);
        var outputPath = _storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await FFmpeg.Conversions.New()
                        .AddStream(inputFile.VideoStreams.First()
                                            .SetSize(1920, 1080)
                                            .SetSize(640, 480)
                                  )
                        .SetOutput(outputPath)
                        .Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath);
        Assert.Equal(640, mediaInfo.VideoStreams.First().Width);
        Assert.Equal(480, mediaInfo.VideoStreams.First().Height);
    }
}
