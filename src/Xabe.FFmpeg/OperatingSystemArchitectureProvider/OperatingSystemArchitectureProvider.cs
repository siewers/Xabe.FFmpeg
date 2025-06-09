namespace Xabe.FFmpeg;

using System;
using System.Runtime.InteropServices;

internal sealed class OperatingSystemArchitectureProvider : IOperatingSystemArchitectureProvider
{
    public OperatingSystemArchitecture GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm => OperatingSystemArchitecture.Arm,
            Architecture.Arm64 => OperatingSystemArchitecture.Arm64,
            Architecture.X64 => OperatingSystemArchitecture.X64,
            Architecture.X86 => OperatingSystemArchitecture.X86,
            _ => throw new NotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}"),
        };
    }
}
