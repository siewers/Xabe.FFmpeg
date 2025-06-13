namespace Xabe.FFmpeg.Test;

public class ConversionToFormatTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    public static IEnumerable<object[]> JoinFiles =>
    [
        [Resources.MkvWithAudio, Resources.Mp4WithAudio, 23, 1280, 720, "16:9"],
        [Resources.MkvWithAudio, Resources.MkvWithAudio, 19, 320, 240, "4:3"],
        [Resources.MkvWithAudio, Resources.Mp4, 23, 1280, 720, "16:9"],
    ];

    [Theory]
    [InlineData(1, 0, 25)]
    [InlineData(1, 1, 24.889)]
    public async Task ToGifTest(int loopCount, int delay, double framerate)
    {
        // Arrange
        var output = storageFixture.GetTempFileName(FileExtensions.Gif);

        // Act
        _ = await (await FFmpeg.Conversions.FromSnippet.ToGif(Resources.Mp4, output.FullName, loopCount, delay, _testCancellationToken))
                  .SetPreset(ConversionPreset.UltraFast)
                  .Start(_testCancellationToken);

        // Assert
        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Duration.Should().BeCloseTo(13.Seconds().And(480.Milliseconds()), 100.Milliseconds());
            mediaInfo.AudioStreams.Should().BeEmpty();
            mediaInfo.VideoStreams.Should().ContainSingle()
                     .Which.Should().Satisfy<IVideoStream>(videoStream =>
                                                           {
                                                               videoStream.Codec.Should().Be("gif");
                                                               videoStream.Ratio.Should().Be("16:9");
                                                               videoStream.Framerate.Should().Be(framerate);
                                                               videoStream.Width.Should().Be(1280);
                                                               videoStream.Height.Should().Be(720);
                                                           }
                                                          );
        }
    }

    [Fact]
    public async Task ToMp4Test()
    {
        // Arrange
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);

        // Act
        await FFmpeg.Conversions.FromSnippet.ToMp4(Resources.MkvWithAudio, output.FullName, _testCancellationToken)
                    .StartConversion(_testCancellationToken);

        // Assert
        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Duration.Should().Be(9.Seconds().And(880.Milliseconds()));
            mediaInfo.AudioStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("aac");

            mediaInfo.VideoStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("h264");
        }
    }

    [Fact]
    public async Task ToOgvTest()
    {
        // Arrange
        var output = storageFixture.GetTempFileName(FileExtensions.Ogv);

        // Act
        await FFmpeg.Conversions.FromSnippet.ToOgv(Resources.MkvWithAudio, output.FullName, _testCancellationToken)
                    .StartConversion(_testCancellationToken);

        // Assert
        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Duration.Should().Be(9.Seconds().And(880.Milliseconds()));
            mediaInfo.AudioStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("vorbis");

            mediaInfo.VideoStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("theora");
        }
    }

    [Fact]
    public async Task ToTsTest()
    {
        // Arrange
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);

        // Act
        await FFmpeg.Conversions.FromSnippet.ToTs(Resources.Mp4WithAudio, output.FullName, _testCancellationToken)
                    .StartConversion(_testCancellationToken);

        // Assert
        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Duration.Should().Be(13.Seconds().And(512.Milliseconds()));
            mediaInfo.AudioStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("mp2");

            mediaInfo.VideoStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("mpeg2video");
        }
    }

    [Fact]
    public async Task ToWebMTest()
    {
        // Arrange
        var output = storageFixture.GetTempFileName(FileExtensions.WebM);

        // Act
        _ = await (await FFmpeg.Conversions.FromSnippet.ToWebM(Resources.Mp4WithAudio, output.FullName, _testCancellationToken))
                  .SetPreset(ConversionPreset.UltraFast)
                  .Start(_testCancellationToken);

        // Assert
        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Duration.Should().Be(13.Seconds().And(507.Milliseconds()));
            mediaInfo.AudioStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("vorbis");

            mediaInfo.VideoStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("vp8");
        }
    }

    [Fact]
    public async Task ConversionWithoutSpecificFormat()
    {
        // Arrange
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);

        // Act
        await Conversion.ConvertAsync(Resources.MkvWithAudio, output.FullName, cancellationToken: _testCancellationToken)
                        .StartConversion(_testCancellationToken);

        // Assert
        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);

        using (new AssertionScope())
        {
            mediaInfo.Duration.Should().Be(9.Seconds().And(880.Milliseconds()));
            mediaInfo.AudioStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("aac");

            mediaInfo.VideoStreams.Should().ContainSingle()
                     .Which.Codec.Should().Be("h264");
        }
    }
}
