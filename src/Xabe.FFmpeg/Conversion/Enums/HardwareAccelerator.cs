// ReSharper disable InconsistentNaming

namespace Xabe.FFmpeg;

using NetEscapades.EnumGenerators;

/// <summary>
///     Hardware accelerators ("ffmpeg -hwaccels")
/// </summary>
[PublicAPI]
[EnumExtensions]
public enum HardwareAccelerator
{
    /// <summary>
    ///     d3d11va
    /// </summary>
    d3d11va,

    /// <summary>
    ///     Automatically select the hardware acceleration method.
    /// </summary>
    auto,

    /// <summary>
    ///     Use DXVA2 (DirectX Video Acceleration) hardware acceleration.
    /// </summary>
    dxva2,

    /// <summary>
    ///     Use the Intel QuickSync Video acceleration for video transcoding.
    /// </summary>
    qsv,

    /// <summary>
    ///     cuvid
    /// </summary>
    cuvid,

    /// <summary>
    ///     Use VDPAU (Video Decode and Presentation API for Unix) hardware acceleration.
    /// </summary>
    vdpau,

    /// <summary>
    ///     Use VAAPI (Video Acceleration API) hardware acceleration.
    /// </summary>
    vaapi,

    /// <summary>
    ///
    /// </summary>
    libmfx,
}
