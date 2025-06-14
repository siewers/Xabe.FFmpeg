// ReSharper disable InconsistentNaming

namespace Xabe.FFmpeg;

using NetEscapades.EnumGenerators;

[PublicAPI]
[EnumExtensions]
public enum AmplitudeScale
{
    lin,
    sqrt,
    cbrt,
    log,
}
