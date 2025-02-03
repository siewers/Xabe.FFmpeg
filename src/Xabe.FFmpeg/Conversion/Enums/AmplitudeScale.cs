// ReSharper disable InconsistentNaming

namespace Xabe.FFmpeg;

using JetBrains.Annotations;

[PublicAPI]
public enum AmplitudeScale
{
    lin,
    sqrt,
    cbrt,
    log,
}
