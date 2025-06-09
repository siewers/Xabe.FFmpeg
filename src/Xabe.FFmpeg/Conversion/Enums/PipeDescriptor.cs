// ReSharper disable InconsistentNaming

namespace Xabe.FFmpeg;

using NetEscapades.EnumGenerators;

[PublicAPI]
[EnumExtensions]
public enum PipeDescriptor
{
    stdin,
    stdout,
    stderr,
}
