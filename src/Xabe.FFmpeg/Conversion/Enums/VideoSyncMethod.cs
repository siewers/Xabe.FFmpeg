// ReSharper disable InconsistentNaming

namespace Xabe.FFmpeg;

using NetEscapades.EnumGenerators;

[PublicAPI]
[EnumExtensions]
public enum VideoSyncMethod
{
    passthrough,
    cfr,
    vfr,
    drop,
    auto,
}
