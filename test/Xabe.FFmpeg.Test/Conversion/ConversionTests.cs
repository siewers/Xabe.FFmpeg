namespace Xabe.FFmpeg.Test;

using Exceptions;
using Probe;

public sealed class ConversionTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>, IClassFixture<MediaMtxServerFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [Theory]
    [InlineData(Position.UpperRight)]
    [InlineData(Position.BottomRight)]
    [InlineData(Position.Left)]
    [InlineData(Position.Right)]
    [InlineData(Position.Up)]
    [InlineData(Position.BottomLeft)]
    [InlineData(Position.UpperLeft)]
    [InlineData(Position.Center)]
    [InlineData(Position.Bottom)]
    public async Task WatermarkTest(Position position)
    {
        var inputFile = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var outputPath = Path.ChangeExtension(Path.GetTempFileName(), FileExtensions.Mp4);
        var stream = inputFile.VideoStreams.First()
                              .SetWatermark(Resources.PngSample, position);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .SetPreset(ConversionPreset.UltraFast)
                                           .AddStream(stream)
                                           .SetOutput(outputPath)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Should().ContainAll("overlay", Resources.PngSample);
        var mediaInfo = await FFmpeg.GetMediaInfo(outputPath, _testCancellationToken);
        mediaInfo.Duration.Should().Be(9.Seconds());
        mediaInfo.VideoStreams.First().Codec.Should().Be("h264");
        mediaInfo.AudioStreams.Should().BeEmpty();
    }

    [Fact]
    public async Task PipedOutputTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        var conversion = FFmpeg.Conversions.Create()
                               .AddStream(videoStream)
                               .SetOutputFormat(Format.mpegts)
                               .PipeOutput();

        var fs = output.OpenRead();

        try
        {
            conversion.OnVideoDataReceived += async (_, args) => { await fs.WriteAsync(args.Data.AsMemory(0, args.Data.Length), _testCancellationToken); };
            await conversion.Start(_testCancellationToken);
        }
        finally
        {
            await fs.DisposeAsync();
        }

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Path.GetExtension(resultFile.Location.AbsoluteUri).Should().Be(".ts");
    }

    [Fact]
    public async Task SetOutputFormatTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetOutputFormat(Format.mpegts)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Path.GetExtension(resultFile.Location.AbsoluteUri).Should().Be(".ts");
    }

    [Theory]
    [InlineData(Format._3dostr, "3dostr")]
    [InlineData(Format._3g2, "3g2")]
    [InlineData(Format._3gp, "3gp")]
    [InlineData(Format._4xm, "4xm")]
    [InlineData(Format.matroska, "matroska")]
    public async Task SetOutputFormat_ValuesFromEnum_CorrectParams(Format format, string expectedFormat)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        var args = FFmpeg.Conversions.Create()
                         .AddStream(videoStream)
                         .SetOutputFormat(format)
                         .SetOutput(output.FullName)
                         .Build();

        args.Should().Contain($"-f {expectedFormat}");
    }

    [Fact]
    public async Task SetOutputFormat_ValueAsString_CorrectParams()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        var args = FFmpeg.Conversions.Create()
                         .AddStream(videoStream)
                         .SetOutputFormat("matroska")
                         .SetOutput(output.FullName)
                         .Build();

        Assert.Contains("-f matroska", args);
    }

    [Fact]
    public async Task SetOutputFormat_NotExistingFormat_ThrowConversionException()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(videoStream)
                                                                            .SetOutputFormat("notExisting")
                                                                            .SetOutput(output.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConversionExceptionBase>();
    }

    [Theory]
    [InlineData(Format._3dostr, "3dostr")]
    [InlineData(Format._3g2, "3g2")]
    [InlineData(Format._3gp, "3gp")]
    [InlineData(Format._4xm, "4xm")]
    [InlineData(Format.matroska, "matroska")]
    public async Task SetFormat_ValuesFromEnum_CorrectParams(Format format, string expectedFormat)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        var args = FFmpeg.Conversions.Create()
                         .AddStream(videoStream)
                         .SetInputFormat(format)
                         .SetOutput(output.FullName)
                         .Build();

        args.Should().Contain($"-f {expectedFormat}");
    }

    [Fact]
    public async Task SetFormat_ValueAsString_CorrectParams()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        var args = FFmpeg.Conversions.Create()
                         .AddStream(videoStream)
                         .SetInputFormat("matroska")
                         .SetOutput(output.FullName)
                         .Build();

        args.Should().Contain("-f matroska");
    }

    [Fact]
    public async Task SetFormat_NotExistingFormat_ThrowConversionException()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Ts);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .AddStream(videoStream)
                                                                            .SetInputFormat("notExisting")
                                                                            .SetOutput(output.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConversionExceptionBase>();
    }

    [Fact]
    public async Task SetInputAndOutputFormatTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Avi);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);
        await FFmpeg.Conversions.Create()
                    .SetInputFormat(Format.matroska)
                    .AddStream(videoStream)
                    .SetOutputFormat(Format.avi)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Codec.Should().Be("mpeg4");
        Path.GetExtension(resultFile.Location.AbsoluteUri).Should().Be(".avi");
    }

    [Fact]
    public async Task SetOutputPixelFormatTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().PixelFormat.Should().Be("yuv420p");
    }

    [Theory]
    [InlineData(Hash.MD5, 37L)]
    [InlineData(Hash.murmur3, 41L)]
    [InlineData(Hash.RIPEMD128, 43L)]
    [InlineData(Hash.RIPEMD160, 51L)]
    [InlineData(Hash.RIPEMD256, 75L)]
    [InlineData(Hash.RIPEMD320, 91L)]
    [InlineData(Hash.SHA160, 48L)]
    [InlineData(Hash.SHA224, 64L)]
    [InlineData(Hash.SHA256, 72L)]
    [InlineData(Hash.SHA512_224, 68L)]
    [InlineData(Hash.SHA512_256, 76L)]
    [InlineData(Hash.SHA384, 104L)]
    [InlineData(Hash.SHA512, 136L)]
    [InlineData(Hash.CRC32, 15L)]
    [InlineData(Hash.adler32, 17L)]
    public async Task SetHashFormatTest(Hash hashFormat, long expectedLength)
    {
        var fileExtension = FileExtensions.Txt;

        if (hashFormat == Hash.SHA256)
        {
            fileExtension = FileExtensions.Sha256;
        }

        //string output = _storageFixture.GetTempFileName(fileExtension);
        var output = storageFixture.GetTempFileName(fileExtension);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.copy);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.copy);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .AddStream(audioStream)
                    .SetHashFormat(hashFormat)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        output.Extension.Should().Be(fileExtension);
        output.Length.Should().Be(expectedLength);
    }

    [Fact]
    public async Task SetHashFormat_HashInString_CorrectLength()
    {
        var fileExtension = FileExtensions.Txt;

        var output = storageFixture.GetTempFileName(fileExtension);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.copy);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.copy);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .AddStream(audioStream)
                    .SetOutputFormat(Format.hash)
                    .SetHashFormat("SHA512/224")
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        output.Extension.Should().Be(fileExtension);
        output.Length.Should().Be(68L);
    }

    [RunnableInDebugOnly]
    public async Task GetScreenCaptureTest_UseVideoSize_EverythingIsCorrect()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddDesktopStream(VideoSize.Qcif, 29.833, 10, 10)
                    .SetInputTime(TimeSpan.FromSeconds(3))
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Codec.Should().Be("h264");
        Assert.Equal(29, (int)resultFile.VideoStreams.First().Framerate);
        Assert.Equal(3, resultFile.VideoStreams.First().Duration.Seconds);
        Assert.Equal(176, resultFile.VideoStreams.First().Width);
        Assert.Equal(144, resultFile.VideoStreams.First().Height);
    }

    [RunnableInDebugOnly]
    public async Task GetScreenCaptureTest_UseVideoSizeAsString_EverythingIsCorrect()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddDesktopStream("176x144", 30, 10, 10)
                    .SetInputTime(TimeSpan.FromSeconds(3))
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal("h264", resultFile.VideoStreams.First().Codec);
        Assert.Equal(30, resultFile.VideoStreams.First().Framerate);
        Assert.Equal(3, resultFile.VideoStreams.First().Duration.Seconds);
        Assert.Equal(176, resultFile.VideoStreams.First().Width);
        Assert.Equal(144, resultFile.VideoStreams.First().Height);
    }

    [Fact]
    public async Task SetVideoCodecTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal("mpeg4", resultFile.VideoStreams.First().Codec);
    }

    [Fact]
    public async Task SetAudioCodecTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.ac3);
        await FFmpeg.Conversions.Create()
                    .AddStream(audioStream)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal("ac3", resultFile.AudioStreams.First().Codec);
    }

    [Fact]
    public async Task SetInputTimeTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First();
        var videoStream = info.VideoStreams.First();
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .AddStream(audioStream)
                    .SetInputTime(TimeSpan.FromSeconds(5))
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(5, resultFile.AudioStreams.First().Duration.Seconds);
        Assert.Equal(5, resultFile.VideoStreams.First().Duration.Seconds);
    }

    [Fact]
    public async Task SetOutputTimeTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First();
        var videoStream = info.VideoStreams.First();
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .AddStream(audioStream)
                    .SetOutputTime(TimeSpan.FromSeconds(5))
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(TimeSpan.FromSeconds(5), resultFile.AudioStreams.First().Duration);
        Assert.Equal(TimeSpan.FromSeconds(5), resultFile.VideoStreams.First().Duration);
    }

    [Fact]
    public async Task SetAudioBitrateTest()
    {
        const long targetBitrate = 128000;
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.ac3);
        await FFmpeg.Conversions.Create()
                    .AddStream(audioStream)
                    .SetAudioBitrate(targetBitrate)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        const long lowerBound = (long)(128000 * 0.95);
        const long upperBound = (long)(128000 * 1.05);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.AudioStreams.First().Bitrate.Should().BeInRange(lowerBound, upperBound);
    }

    [Fact]
    public async Task SetLibH264VideoBitrateTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.libx264);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetVideoBitrate(1500000)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        const long lowerBound = (long)(1500000 * 0.95);
        const long upperBound = (long)(1500000 * 1.05);
        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Bitrate.Should().BeInRange(lowerBound, upperBound);
    }

    [Fact]
    public async Task SetH264VideoBitrateTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.h264);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetVideoBitrate(1500000)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        const long lowerBound = (long)(1500000 * 0.95);
        const long upperBound = (long)(1500000 * 1.05);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Bitrate.Should().BeInRange(lowerBound, upperBound);
    }

    [Fact]
    public async Task SetNonH264VideoBitrateTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetVideoBitrate(1500000)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        const long lowerBound = (long)(1500000 * 0.95);
        const long upperBound = (long)(1500000 * 1.05);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Bitrate.Should().BeInRange(lowerBound, upperBound);
    }

    [Theory]
    [InlineData(FileExtensions.Png)]
    [InlineData(FileExtensions.WebP)]
    [InlineData(FileExtensions.Jpg)]
    public async Task ExtractEveryNthFrameTest(string extension)
    {
        var tempPath = storageFixture.GetTempDirectory();

        string OutputBuilder(string number)
        {
            return Path.Combine(tempPath, number + extension);
        }

        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.png);

        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .ExtractEveryNthFrame(10, OutputBuilder)
                    .Start(_testCancellationToken);

        var outputFilesCount = Directory.EnumerateFiles(tempPath).Count();

        Assert.Equal(26, outputFilesCount);
    }

    [Theory]
    [InlineData(FileExtensions.Png)]
    [InlineData(FileExtensions.WebP)]
    [InlineData(FileExtensions.Jpg)]
    public async Task ExtractNthFrameTest(string extension)
    {
        var tempPath = storageFixture.GetTempDirectory();

        string OutputBuilder(string number)
        {
            return Path.Combine(tempPath, number + extension);
        }

        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.png);

        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .ExtractNthFrame(10, OutputBuilder)
                    .Start(_testCancellationToken);

        var outputFilesCount = Directory.EnumerateFiles(tempPath).Count();

        Assert.Equal(1, outputFilesCount);
    }

    [Fact]
    public async Task BuildVideoFromImagesTest()
    {
        var files = Directory.EnumerateFiles(Resources.Images).ToList();
        var builder = new InputBuilder();
        var inputBuilder = builder.PrepareInputFiles(files, out var preparedFilesDir);
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .SetInputFramerate(1)
                    .BuildVideoFromImages(1, inputBuilder)
                    .SetFramerate(1)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var preparedFilesCount = Directory.EnumerateFiles(preparedFilesDir).ToList().Count;

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(builder.FileList.Count, preparedFilesCount);
        Assert.Equal(TimeSpan.FromSeconds(12), resultFile.VideoStreams.First().Duration);
        Assert.Equal(1, resultFile.VideoStreams.First().Framerate);
        resultFile.VideoStreams.First().PixelFormat.Should().Be("yuv420p");
    }

    [Fact]
    public async Task BuildVideoFromImagesAndAudioTest()
    {
        var files = Directory.EnumerateFiles(Resources.Images).ToList();
        var builder = new InputBuilder();
        var audioInfo = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = audioInfo.AudioStreams.First();
        var inputBuilder = builder.PrepareInputFiles(files, out var preparedFilesDir);

        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .SetInputFramerate(1)
                    .BuildVideoFromImages(1, inputBuilder)
                    .SetFramerate(1)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    .AddStream(audioStream)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var preparedFilesCount = Directory.EnumerateFiles(preparedFilesDir).ToList().Count;

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(builder.FileList.Count, preparedFilesCount);
        Assert.Equal(TimeSpan.FromSeconds(12), resultFile.VideoStreams.First().Duration);
        Assert.Equal(1, resultFile.VideoStreams.First().Framerate);
        resultFile.VideoStreams.First().PixelFormat.Should().Be("yuv420p");
        Assert.Single(resultFile.AudioStreams);
    }

    [Theory]
    [InlineData(PixelFormat._0bgr, "0bgr")]
    [InlineData(PixelFormat._0rgb, "0rgb")]
    [InlineData(PixelFormat.yuv410p, "yuv410p")]
    public void SetPixelFormat_DataFromEnum_CorrectArgs(PixelFormat pixelFormat, string expectedPixelFormat)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var args = FFmpeg.Conversions.Create()
                         .SetPixelFormat(pixelFormat)
                         .SetOutput(output.FullName)
                         .Build();

        args.Should().Contain($"-pix_fmt {expectedPixelFormat}");
    }

    [Fact]
    public void SetPixelFormat_DataFromString_CorrectArgs()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var args = FFmpeg.Conversions.Create()
                         .SetPixelFormat("testFormat")
                         .SetOutput(output.FullName)
                         .Build();

        args.Should().Contain("-pix_fmt testFormat");
    }

    [Fact]
    public async Task SetPixelFormat_NotExistingFormat_ThrowConversionException()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var exception = await Record.ExceptionAsync(async () => await FFmpeg.Conversions.Create()
                                                                            .SetPixelFormat("notExistingFormat")
                                                                            .SetOutput(output.FullName)
                                                                            .Start(_testCancellationToken)
                                                   );

        exception.Should().NotBeNull();
        exception.Should().BeOfType<ConversionExceptionBase>();
    }

    [Fact]
    public async Task BuildVideoFromImagesListTest()
    {
        var files = Directory.EnumerateFiles(Resources.Images).ToList();
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .SetInputFramerate(1)
                    .BuildVideoFromImages(files)
                    .SetFramerate(1)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(TimeSpan.FromSeconds(12), resultFile.VideoStreams.First().Duration);
        Assert.Equal(1, resultFile.VideoStreams.First().Framerate);
        resultFile.VideoStreams.First().PixelFormat.Should().Be("yuv420p");
    }

    [Fact]
    public async Task BuildVideoFromImagesListAndAudioTest()
    {
        var files = Directory.EnumerateFiles(Resources.Images).ToList();
        var audioInfo = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = audioInfo.AudioStreams.First();
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .SetInputFramerate(1)
                    .BuildVideoFromImages(files)
                    .SetFramerate(1)
                    .SetPixelFormat(PixelFormat.yuv420p)
                    .AddStream(audioStream)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.Equal(TimeSpan.FromSeconds(12), resultFile.VideoStreams.First().Duration);
        Assert.Equal(1, resultFile.VideoStreams.First().Framerate);
        resultFile.VideoStreams.First().PixelFormat.Should().Be("yuv420p");
        Assert.Single(resultFile.AudioStreams);
    }

    [Fact]
    public async Task OverwriteFilesTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.ac3);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(audioStream)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Should().Contain("-n ");

        var secondConversionResult = await FFmpeg.Conversions.Create()
                                                 .AddStream(audioStream)
                                                 .SetOverwriteOutput(true)
                                                 .SetOutput(output.FullName)
                                                 .Start(_testCancellationToken);

        secondConversionResult.Arguments.Should().Contain(" -y ");
        secondConversionResult.Arguments.Should().NotContain(" -n ");
    }

    [Fact]
    public async Task OverwriteFilesExceptionTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.ac3);

        var conversionResult = await Convert();
        conversionResult.Arguments.Should().ContainAll("-n", output.FullName);

        await FluentActions.Awaiting(Convert).Should().ThrowAsync<GenericConversionException>();
        return;

        async Task<IConversionResult> Convert()
        {
            return await FFmpeg.Conversions.Create()
                               .AddStream(audioStream)
                               .SetOutput(output.FullName)
                               .Start(_testCancellationToken);
        }
    }

    [RunnableInDebugOnly]
    public async Task UseHardwareAcceleration()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await (await FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithAudio, output.FullName, cancellationToken: _testCancellationToken)).UseHardwareAcceleration(HardwareAccelerator.auto, VideoCodec.h264_cuvid, VideoCodec.h264_nvenc, 0).Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Codec.Should().Be("h264");
    }

    [Theory]
    [InlineData("a16f0cb5c0354b6197e9f3bc3108c017")]
    public async Task MissingHardwareAccelerator(string hardwareAccelerator)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var exception = await Record.ExceptionAsync(async () =>
                                                    {
                                                        await (await FFmpeg.Conversions.FromSnippet
                                                                           .Convert(Resources.MkvWithAudio, output.FullName, cancellationToken: _testCancellationToken))
                                                              .UseHardwareAcceleration(hardwareAccelerator, "h264_cuvid", "h264_nvenc")
                                                              .Start(_testCancellationToken);
                                                    }
                                                   );

        exception.Should().NotBeNull();
        exception.Should().BeOfType<HardwareAcceleratorNotFoundException>();
    }

    [RunnableInDebugOnly]
    public async Task UnknownDecoderException()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        info.VideoStreams.First().SetCodec(VideoCodec.mpeg4);

        IConversionResult? result = null;
        var exception = await Record.ExceptionAsync(async () =>
                                                    {
                                                        var snippet = await FFmpeg.Conversions.FromSnippet.Convert(Resources.MkvWithAudio, output.FullName, cancellationToken: _testCancellationToken);
                                                        var conversion = snippet.UseHardwareAcceleration(HardwareAccelerator.auto, VideoCodec.h264_nvenc, VideoCodec.h264_cuvid);
                                                        result = await conversion.Start(_testCancellationToken);
                                                    }
                                                   );

        result.Should().BeNull("Result cannot be instantiated. Code should fail.");
        exception.Should().NotBeNull("No exception thrown.");
        exception.Should().BeOfType<UnknownDecoderException>();
    }

    [Fact]
    public async Task Conversion_CancellationOccurs_ExceptionWasThrown()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.WebM);

        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_testCancellationToken);
        var conversion = await Conversion.ToWebM(Resources.Mp4WithAudio, output.FullName, cancellationTokenSource.Token);

        await cancellationTokenSource.CancelAsync();
        await FluentActions.Awaiting(ConversionTask).Should().ThrowAsync<OperationCanceledException>();
        return;

        Task<IConversionResult> ConversionTask()
        {
            return conversion.SetPreset(ConversionPreset.UltraFast)
                             .Start(cancellationTokenSource.Token);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(0)]
    public async Task UseMultiThreadTest(int expectedThreadsCount)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .UseMultiThread(expectedThreadsCount)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Codec.Should().Be("h264");
        conversionResult.Arguments.Should().Contain($"-threads {expectedThreadsCount}");
    }

    [Fact]
    public async Task UseMultiThreadTest_WithoutThreadCount_AllThreads()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .UseMultiThread(true)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Codec.Should().Be("h264");
        conversionResult.Arguments.Should().Contain($"-threads {Math.Min(Environment.ProcessorCount, 16)}");
    }

    [Fact]
    public async Task UseMultiThreadTest_WithoutMultithread_OneThreadOnly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .UseMultiThread(false)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.VideoStreams.First().Codec.Should().Be("h264");
        conversionResult.Arguments.Should().Contain("-threads 1");
    }

    [Fact]
    public async Task AddPreParameterTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .AddParameter("-re", ParameterPosition.PreInput)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Should().Contain("-re");
    }

    [Fact]
    public async Task TryConvertMedia_NoFilesInFFmpegDirectory_ThrowFFmpegNotFoundException()
    {
        var path = FFmpeg.ExecutablesPath;

        try
        {
            FFmpeg.SetExecutablesPath(storageFixture.TempDirectory.FullName);

            var exception = await Record.ExceptionAsync(async () => await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken));

            exception.Should().NotBeNull().And.BeOfType<FFmpegNotFoundException>();
        }
        finally
        {
            FFmpeg.SetExecutablesPath(path);
        }
    }

    [Fact]
    public async Task ConvertSloMoTest()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.SloMoMp4, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.h264);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetFramerate(videoStream.Framerate)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        var resultVideoStream = resultFile.VideoStreams.First();

        Path.GetExtension(resultFile.Location.AbsoluteUri).Should().Be(".mp4");

        // It does not have to be the same
        resultVideoStream.Framerate.Should().Be(116.244);
        resultVideoStream.Duration.Should().BeCloseTo(3.Seconds(), 50.Milliseconds());
    }

    [Theory]
    [InlineData(VideoSyncMethod.cfr)]
    [InlineData(VideoSyncMethod.drop)]
    [InlineData(VideoSyncMethod.passthrough)]
    [InlineData(VideoSyncMethod.vfr)]
    public async Task AddVsync_CorrectValues_VsyncMethodIsSet(VideoSyncMethod vsyncMethod)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First().SetCodec(VideoCodec.copy))
                                           .SetVideoSyncMethod(vsyncMethod)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Should().Contain($"-vsync {vsyncMethod}");
    }

    [Fact]
    public async Task AddVsync_AutoMethod_VsyncMethodIsSetCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .SetVideoSyncMethod(VideoSyncMethod.auto)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Should().Contain("-vsync -1");
    }

    [Fact]
    public async Task SendToRtspServer_MinimumConfiguration_FileIsBeingStreamed()
    {
        // Arrange

        var output = new Uri("rtsp://127.0.0.1:8554/newFile");

        // Act
        await FFmpeg.Conversions.FromSnippet.SendToRtspServer(Resources.Mp4, output, _testCancellationToken)
                    .StartConversion(_testCancellationToken);

        await Task.Delay(2.Seconds(), _testCancellationToken);

        // Assert
        var info = await MediaInfo.Get(output, _testCancellationToken);

        info.Streams.Should().ContainSingle();
    }

    [RunnableInDebugOnly]
    public async Task GetAvailableDevices_SomeDevicesAreConnected_ReturnAllDevices()
    {
        // Arrange
        var devices = await FFmpeg.GetAvailableDevices();

        // Assert
        devices.Should().HaveCount(2);
        devices.Should().ContainSingle(device => device.Name == "Logitech HD Webcam C270");
    }

    [RunnableInDebugOnly]
    public async Task SendDesktopToRtspServer_MinimumConfiguration_DesktopIsBeingStreamed()
    {
        // Arrange
        var output = new Uri("rtsp://127.0.0.1:8554/desktop");

        // Act
        _ = FFmpeg.Conversions.FromSnippet
                  .SendDesktopToRtspServer(output)
                  .Start(_testCancellationToken);

        //Give it some time to warm up
        await Task.Delay(2.Seconds(), _testCancellationToken);

        // Assert
        var info = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        info.Streams.Should().ContainSingle();
    }

    [RunnableInDebugOnly]
    public async Task GetScreenCaptureTest_UseNewAddDesktopStream_EverythingIsCorrect()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.Create()
                    .AddDesktopStream(VideoSize.Cga, 29.833, 0, 0)
                    .SetInputTime(TimeSpan.FromSeconds(3))
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);

        resultFile.VideoStreams.Should().ContainSingle()
                  .Which.Should().Satisfy<IVideoStream>(stream =>
                                                        {
                                                            stream.Codec.Should().Be("h264");
                                                            stream.Duration.Should().BeCloseTo(3.Seconds(), 50.Milliseconds());
                                                            stream.Width.Should().Be(320);
                                                            stream.Height.Should().Be(200);
                                                            stream.Framerate.Should().Be(29.833);
                                                        }
                                                       );
    }

    [Fact]
    public async Task Conversion_MillisecondsInTimeSpan_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First();
        var videoStream = info.VideoStreams.First();
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .AddStream(audioStream)
                    .SetInputTime(TimeSpan.FromMilliseconds(1500))
                    .SetOutputTime(TimeSpan.FromMilliseconds(1500))
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.AudioStreams.First().Duration.Should().Be(1500.Milliseconds());
    }

    [Fact]
    public async Task Conversion_SpacesInOutputPath_WorksCorrectly()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var nameWithSpaces = output.Name.Replace("-", " ");
        output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        await FFmpeg.Conversions.Create()
                    .AddStream(info.VideoStreams.First())
                    .AddParameter("-re", ParameterPosition.PreInput)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var outputMediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        outputMediaInfo.Streams.Should().NotBeNull();
    }

    [Theory]
    [InlineData("'")]
    [InlineData("\"")]
    public async Task Conversion_OutputPathEscaped_WorksCorrectly(string escapeCharacter)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var nameWithSpaces = output.Name.Replace("-", " ");
        output = output.Replace(nameWithSpaces.Replace(" ", "-"), nameWithSpaces);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        await FFmpeg.Conversions.Create()
                    .AddStream(info.VideoStreams.First())
                    .AddParameter("-re", ParameterPosition.PreInput)
                    .SetOutput($"{escapeCharacter}{output}{escapeCharacter}")
                    .Start(_testCancellationToken);

        var outputMediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        outputMediaInfo.Streams.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Crime d'Amour.mp4")]
    public async Task Conversion_SpecialCharactersInName_WorksCorrectly(string outputFileName)
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        output = output.Replace(output.FullName, outputFileName);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddStream(info.VideoStreams.First())
                                           .AddParameter("-re", ParameterPosition.PreInput)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        var outputMediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        outputMediaInfo.Streams.Should().NotBeNull();
        conversionResult.Arguments.Should().Contain("Crime d'Amour");
    }

    [Fact]
    public async Task ExtractEveryNthFrame_OutputDirectoryNotExists_OutputDirectoryIsCreated()
    {
        var tempPath = Path.Combine(storageFixture.GetTempDirectory(), Guid.NewGuid().ToString());

        string OutputBuilder(string number)
        {
            return Path.Combine(tempPath, number + FileExtensions.Png);
        }

        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.png);

        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .ExtractEveryNthFrame(10, OutputBuilder)
                    .Start(_testCancellationToken);

        var outputFilesCount = Directory.EnumerateFiles(tempPath).Count();

        outputFilesCount.Should().Be(26);
    }

    [Fact]
    public async Task Conversion_OutputDirectoryNotExists_OutputDirectoryIsCreated()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.Mp4, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.h264);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetFramerate(videoStream.Framerate)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        resultFile.Streams.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Conversion_DoubleNestedNotExistingDirectory_OutputDirectoryIsCreated()
    {
        var tempPath = storageFixture.GetTempDirectory();
        var output = new FileInfo(Path.Combine(tempPath, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid() + FileExtensions.Mp4));
        var info = await FFmpeg.GetMediaInfo(Resources.Mp4, _testCancellationToken);
        var videoStream = info.VideoStreams.First().SetCodec(VideoCodec.h264);
        await FFmpeg.Conversions.Create()
                    .AddStream(videoStream)
                    .SetFramerate(videoStream.Framerate)
                    .SetOutput(output.FullName)
                    .Start(_testCancellationToken);

        var resultFile = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.NotEmpty(resultFile.Streams);
    }

    [Fact]
    public async Task Conversion_RunItSecondTime_ItWorks()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);
        var audioStream = info.AudioStreams.First().SetCodec(AudioCodec.ac3);

        var conversion = FFmpeg.Conversions.Create()
                               .AddStream(audioStream)
                               .SetOutput(output.FullName);

        await conversion.Start(_testCancellationToken);

        var secondOutput = storageFixture.GetTempFileName(FileExtensions.Mkv);
        var exception = await Record.ExceptionAsync(async () => await conversion.SetOutput(secondOutput.FullName).Start(_testCancellationToken));

        exception.Should().BeNull();
    }

    [Fact]
    public async Task Conversion_EverythingIsPassedAsAdditionalParameter_EverythingWorks()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddParameter($"-ss 00:00:01 -t 00:00:03 -i {Resources.Mp4} -ss 00:00:05 -t 00:00:03 -i {Resources.Mp4}", ParameterPosition.PreInput)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Split(" ").Count(x => x == "-ss").Should().Be(2);
        conversionResult.Arguments.Split(" ").Count(x => x == "-t").Should().Be(2);
    }

    [Fact]
    public async Task Conversion_EverythingIsPassedAsAdditionalParameters_EverythingWorks()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);

        var conversionResult = await FFmpeg.Conversions.Create()
                                           .AddParameter("-ss 00:00:01", ParameterPosition.PreInput)
                                           .AddParameter("-t 00:00:03", ParameterPosition.PreInput)
                                           .AddParameter($"-i {Resources.Mp4}", ParameterPosition.PreInput)
                                           .AddParameter("-ss 00:00:05", ParameterPosition.PreInput)
                                           .AddParameter("-t 00:00:03", ParameterPosition.PreInput)
                                           .AddParameter($"-i {Resources.Mp4}", ParameterPosition.PreInput)
                                           .SetOutput(output.FullName)
                                           .Start(_testCancellationToken);

        conversionResult.Arguments.Split(" ").Count(x => x == "-ss").Should().Be(2);
        conversionResult.Arguments.Split(" ").Count(x => x == "-t").Should().Be(2);
    }

    [Fact]
    public async Task Conversion_FileNameWithoutDirectory_NewFileIsCreatedInCurrentDirectory()
    {
        var tempPath = storageFixture.GetTempDirectory();
        Directory.SetCurrentDirectory(tempPath);
        var tempName = Guid.NewGuid().ToString();

        var info = await FFmpeg.GetMediaInfo(Resources.MkvWithAudio, _testCancellationToken);

        await FFmpeg.Conversions.Create()
                    .AddStreams(info.VideoStreams)
                    .SetOutput($"{tempName}.mp4")
                    .Start(_testCancellationToken);

        File.Exists(Path.Combine(tempPath, $"{tempName}.mp4")).Should().BeTrue();
    }
}
