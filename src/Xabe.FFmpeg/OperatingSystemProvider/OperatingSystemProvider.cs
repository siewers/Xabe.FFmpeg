namespace Xabe.FFmpeg;

using System.Runtime.InteropServices;

internal sealed class OperatingSystemProvider : IOperatingSystemProvider
{
    public OperatingSystem GetOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OperatingSystem.Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OperatingSystem.Osx;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OperatingSystem.Linux;

            // TODO : How to distinct Tizen / Raspberry architecture
            // Linux (Armet) (Tizen)
            // Linux (LinuxArmhf) (for glibc based OS) -> Raspberry Pi
        }

        throw new InvalidOperationException("Missing system type and architecture.");
    }
}
