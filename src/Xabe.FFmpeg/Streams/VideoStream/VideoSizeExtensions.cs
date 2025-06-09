namespace Xabe.FFmpeg;

using System;

internal static class VideoSizeExtensions
{
    public static string ToFFmpegFormat(this VideoSize videoSize)
    {
        return videoSize switch
        {
            VideoSize.Ntsc => "720x480",
            VideoSize.Pal => "720x576",
            VideoSize.Qntsc => "352x240",
            VideoSize.Qpal => "352x288",
            VideoSize.Sntsc => "640x480",
            VideoSize.Spal => "768x576",
            VideoSize.Film => "352x240",
            VideoSize.NtscFilm => "352x240",
            VideoSize.Sqcif => "128x96",
            VideoSize.Qcif => "176x144",
            VideoSize.Cif => "352x288",
            VideoSize._4Cif => "704x576",
            VideoSize._16cif => "1408x1152",
            VideoSize.Qqvga => "160x120",
            VideoSize.Qvga => "320x240",
            VideoSize.Vga => "640x480",
            VideoSize.Svga => "800x600",
            VideoSize.Xga => "1024x768",
            VideoSize.Uxga => "1600x1200",
            VideoSize.Qxga => "2048x1536",
            VideoSize.Sxga => "1280x1024",
            VideoSize.Qsxga => "2560x2048",
            VideoSize.Hsxga => "5120x4096",
            VideoSize.Wvga => "852x480",
            VideoSize.Wxga => "1366x768",
            VideoSize.Wsxga => "1600x1024",
            VideoSize.Wuxga => "1920x1200",
            VideoSize.Woxga => "2560x1600",
            VideoSize.Wqsxga => "3200x2048",
            VideoSize.Wquxga => "3840x2400",
            VideoSize.Whsxga => "6400x4096",
            VideoSize.Whuxga => "7680x4800",
            VideoSize.Cga => "320x200",
            VideoSize.Ega => "640x350",
            VideoSize.Hd480 => "852x480",
            VideoSize.Hd720 => "1280x720",
            VideoSize.Hd1080 => "1920x1080",
            VideoSize._2K => "2048x1080",
            VideoSize._2Kflat => "1998x1080",
            VideoSize._2Kscope => "2048x858",
            VideoSize._4K => "4096x2160",
            VideoSize._4Kflat => "3996x2160",
            VideoSize._4Kscope => "4096x1716",
            VideoSize.Nhd => "640x360",
            VideoSize.Hqvga => "240x160",
            VideoSize.Wqvga => "400x240",
            VideoSize.Fwqvga => "432x240",
            VideoSize.Hvga => "480x320",
            VideoSize.Qhd => "960x540",
            VideoSize._2Kdci => "2048x1080",
            VideoSize._4Kdci => "4096x2160",
            VideoSize.Uhd2160 => "3840x2160",
            VideoSize.Uhd4320 => "7680x4320",
            _ => throw new InvalidOperationException(),
        };
    }
}
