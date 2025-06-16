namespace Xabe.FFmpeg;

using System.ComponentModel.DataAnnotations;
using NetEscapades.EnumGenerators;

/// <summary>
///     Video size
/// </summary>
[PublicAPI]
[EnumExtensions]
public enum VideoSize
{
    /// <summary>
    ///     720x480
    /// </summary>
    [Display(Name = "720x480")] Ntsc,

    /// <summary>
    ///     720x576
    /// </summary>
    [Display(Name = "720x576")] Pal,

    /// <summary>
    ///     352x240
    /// </summary>
    [Display(Name = "352x240")] Qntsc,

    /// <summary>
    ///     352x288
    /// </summary>
    [Display(Name = "352x288")] Qpal,

    /// <summary>
    ///     640x480
    /// </summary>
    [Display(Name = "640x480")] Sntsc,

    /// <summary>
    ///     768x576
    /// </summary>
    [Display(Name = "768x576")] Spal,

    /// <summary>
    ///     352x240
    /// </summary>
    [Display(Name = "352x240")] Film,

    /// <summary>
    ///     352x240
    /// </summary>
    [Display(Name = "352x240")] NtscFilm,

    /// <summary>
    ///     128x96
    /// </summary>
    [Display(Name = "128x96")] Sqcif,

    /// <summary>
    ///     176x144
    /// </summary>
    [Display(Name = "176x144")] Qcif,

    /// <summary>
    ///     352x288
    /// </summary>
    [Display(Name = "352x288")] Cif,

    /// <summary>
    ///     704x576
    /// </summary>
    [Display(Name = "704x576")] _4Cif,

    /// <summary>
    ///     1408x1152
    /// </summary>
    [Display(Name = "1408x1152")] _16cif,

    /// <summary>
    ///     160x120
    /// </summary>
    [Display(Name = "160x120")] Qqvga,

    /// <summary>
    ///     320x240
    /// </summary>
    [Display(Name = "320x240")] Qvga,

    /// <summary>
    ///     640x480
    /// </summary>
    [Display(Name = "640x480")] Vga,

    /// <summary>
    ///     800x600
    /// </summary>
    [Display(Name = "800x600")] Svga,

    /// <summary>
    ///     1024x768
    /// </summary>
    [Display(Name = "1024x768")] Xga,

    /// <summary>
    ///     1600x1200
    /// </summary>
    [Display(Name = "1600x1200")] Uxga,

    /// <summary>
    ///     2048x1536
    /// </summary>
    [Display(Name = "2048x1536")] Qxga,

    /// <summary>
    ///     1280x1024
    /// </summary>
    [Display(Name = "1280x1024")] Sxga,

    /// <summary>
    ///     2560x2048
    /// </summary>
    [Display(Name = "2560x2048")] Qsxga,

    /// <summary>
    ///     5120x4096
    /// </summary>
    [Display(Name = "5120x4096")] Hsxga,

    /// <summary>
    ///     852x480
    /// </summary>
    [Display(Name = "852x480")] Wvga,

    /// <summary>
    ///     1366x768
    /// </summary>
    [Display(Name = "1366x768")] Wxga,

    /// <summary>
    ///     1600x1024
    /// </summary>
    [Display(Name = "1600x1024")] Wsxga,

    /// <summary>
    ///     1920x1200
    /// </summary>
    [Display(Name = "1920x1200")] Wuxga,

    /// <summary>
    ///     2560x1600
    /// </summary>
    [Display(Name = "2560x1600")] Woxga,

    /// <summary>
    ///     3200x2048
    /// </summary>
    [Display(Name = "3200x2048")] Wqsxga,

    /// <summary>
    ///     3840x2400
    /// </summary>
    [Display(Name = "3840x2400")] Wquxga,

    /// <summary>
    ///     6400x4096
    /// </summary>
    [Display(Name = "6400x4096")] Whsxga,

    /// <summary>
    ///     7680x4800
    /// </summary>
    [Display(Name = "7680x4800")] Whuxga,

    /// <summary>
    ///     320x200
    /// </summary>
    [Display(Name = "320x200")] Cga,

    /// <summary>
    ///     640x350
    /// </summary>
    [Display(Name = "640x350")] Ega,

    /// <summary>
    ///     852x480
    /// </summary>
    [Display(Name = "852x480")] Hd480,

    /// <summary>
    ///     1280x720
    /// </summary>
    [Display(Name = "1280x720")] Hd720,

    /// <summary>
    ///     1920x1080
    /// </summary>
    [Display(Name = "1920x1080")] Hd1080,

    /// <summary>
    ///     2048x1080
    /// </summary>
    [Display(Name = "2048x1080")] _2K,

    /// <summary>
    ///     1998x1080
    /// </summary>
    [Display(Name = "1998x1080")] _2Kflat,

    /// <summary>
    ///     2048x858
    /// </summary>
    [Display(Name = "2048x858")] _2Kscope,

    /// <summary>
    ///     4096x2160
    /// </summary>
    [Display(Name = "4096x2160")] _4K,

    /// <summary>
    ///     3996x2160
    /// </summary>
    [Display(Name = "3996x2160")] _4Kflat,

    /// <summary>
    ///     4096x1716
    /// </summary>
    [Display(Name = "4096x1716")] _4Kscope,

    /// <summary>
    ///     640x360
    /// </summary>
    [Display(Name = "640x360")] Nhd,

    /// <summary>
    ///     240x160
    /// </summary>
    [Display(Name = "240x160")] Hqvga,

    /// <summary>
    ///     400x240
    /// </summary>
    [Display(Name = "400x240")] Wqvga,

    /// <summary>
    ///     432x240
    /// </summary>
    [Display(Name = "432x240")] Fwqvga,

    /// <summary>
    ///     480x320
    /// </summary>
    [Display(Name = "480x320")] Hvga,

    /// <summary>
    ///     960x540
    /// </summary>
    [Display(Name = "960x540")] Qhd,

    /// <summary>
    ///     2048x1080
    /// </summary>
    [Display(Name = "2048x1080")] _2Kdci,

    /// <summary>
    ///     4096x2160
    /// </summary>
    [Display(Name = "4096x2160")] _4Kdci,

    /// <summary>
    ///     3840x2160
    /// </summary>
    [Display(Name = "3840x2160")] Uhd2160,

    /// <summary>
    ///     7680x4320
    /// </summary>
    [Display(Name = "7680x4320")] Uhd4320,
}
