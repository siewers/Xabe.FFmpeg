using FluentAssertions;
using Xunit;

namespace Xabe.FFmpeg.Test;

using Exceptions;

public class FFmpegExceptionCatcherTests
{
    private readonly FFmpegExceptionCatcher _sut = new();

    [Fact]
    public void CatchErrors_UnrecognizedHwacceel_ThrowHardwareAcceleratorNotFoundException()
    {
        //Arrange
        var args = "args";
        var output = "Unrecognized hwaccel: a16f0cb5c0354b6197e9f3bc3108c017. Supported hwaccels: cuda dxva2 qsv d3d11va qsv cuvid";

        //Act
        var exception = Record.Exception(() => FFmpegExceptionCatcher.CatchFFmpegErrors(output, args));

        //Assert
        exception.Should().BeOfType<ConversionExceptionBase>()
                 .Which.Message.Should().Be(output);
        exception.Should().NotBeNull();
        exception!.InnerException.Should().BeOfType<HardwareAcceleratorNotFoundException>()
                  .Which.InputParameters.Should().Be(args);
    }

    [Fact]
    public void CatchErrors_NoFFmpegError_NoExceptionIsThrown()
    {
        //Arrange
        var args = "args";
        var output = "FFmpeg result without exception";

        //Act
        var exception = Record.Exception(() => FFmpegExceptionCatcher.CatchFFmpegErrors(output, args));

        //Assert
        Assert.Null(exception);
    }

    [Fact]
    public void CatchErrors_NoSuitableOutputFormat_ThrowFFmpegNoSuitableOutputFormatFoundException()
    {
        //Arrange
        var args = "args";
        var output = @"Unable to find a suitable output format for 'C:\Users\tomas\AppData\Local\Temp\4da4b324-3e25-42cb-b7b3-f9da041cf20c' C: \Users\tomas\AppData\Local\Temp\4da4b324 - 3e25 - 42cb - b7b3 - f9da041cf20c: Invalid argument";

        //Act
        var exception = Record.Exception(() => FFmpegExceptionCatcher.CatchFFmpegErrors(output, args));

        //Assert
        Assert.IsType<ConversionExceptionBase>(exception);
        Assert.IsType<FFmpegNoSuitableOutputFormatFoundException>(exception.InnerException);
        Assert.Equal(output, exception.Message);
        var conversionException = (ConversionExceptionBase)exception;
        Assert.Equal(args, conversionException.InputParameters);
    }
}
