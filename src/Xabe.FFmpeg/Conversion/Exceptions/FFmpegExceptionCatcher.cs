namespace Xabe.FFmpeg.Exceptions;

using System.Collections.Generic;

internal sealed class FFmpegExceptionCatcher
{
    private static readonly List<ExceptionCheck> Checks = [];

    static FFmpegExceptionCatcher()
    {
        Checks.Add(new ExceptionCheck("Invalid NAL unit size", false, GenericConversionException.Create));
        Checks.Add(new ExceptionCheck("Packet mismatch", true, GenericConversionException.Create));
        Checks.Add(new ExceptionCheck("asf_read_pts failed", true, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("Missing key frame while searching for timestamp", true, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("Old interlaced mode is not supported", true, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("mpeg1video", true, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("Frame rate very high for a muxer not efficiently supporting it", true, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("multiple fourcc not supported", false, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("Unknown decoder", false, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("Failed to open codec in avformat_find_stream_info", false, UnknownDecoderException.Create));
        Checks.Add(new ExceptionCheck("Unrecognized hwaccel: ", false, HardwareAcceleratorNotFoundException.Create));
        Checks.Add(new ExceptionCheck("Unable to find a suitable output format", false, FFmpegNoSuitableOutputFormatFoundException.Create));
        Checks.Add(new ExceptionCheck("is not supported by the bitstream filter", false, InvalidBitstreamFilterException.Create));
    }

    internal static void CatchFFmpegErrors(string output, string args)
    {
        foreach (var check in Checks)
        {
            if (check.CheckLog(output))
            {
                check.Throw(output, args);
            }
        }
    }
}
