namespace Xabe.FFmpeg;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Exceptions;

/// <summary>
///     Wrapper for FFmpeg
/// </summary>
[PublicAPI]
public abstract partial class FFmpeg
{
    private static readonly Lock FFmpegPathLock = new();
    private static readonly Lock FFprobePathLock = new();
    private static string _ffmpegExecutableName = "ffmpeg";
    private static string _ffprobeExecutableName = "ffprobe";
    private static string? _ffmpegPath;
    private static string? _ffprobePath;
    private static string _lastExecutablePath = Guid.NewGuid().ToString();

    /// <summary>
    ///     Initialize new FFmpeg. Search FFmpeg and FFprobe in PATH
    /// </summary>
    protected FFmpeg()
    {
        FindAndValidateExecutables();
    }

    /// <summary>
    ///     Filepath to FFmpeg
    /// </summary>
    protected string? FFmpegPath
    {
        get
        {
            lock (FFmpegPathLock)
            {
                return _ffmpegPath;
            }
        }
        private set
        {
            lock (FFmpegPathLock)
            {
                _ffmpegPath = value;
            }
        }
    }

    /// <summary>
    ///     Filepath to FFprobe
    /// </summary>
    protected string? FFprobePath
    {
        get
        {
            lock (FFprobePathLock)
            {
                return _ffprobePath;
            }
        }
        private set
        {
            lock (FFprobePathLock)
            {
                _ffprobePath = value;
            }
        }
    }

    [MemberNotNull(nameof(FFmpegPath))]
    [MemberNotNull(nameof(FFprobePath))]
    private void FindAndValidateExecutables()
    {
        if (IsPathsSet() && _lastExecutablePath.Equals(ExecutablesPath))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(ExecutablesPath))
        {
            var files = new DirectoryInfo(ExecutablesPath).GetFiles();

            Func<string, string, IFormatProvider, bool> compareMethod = FilterMethod switch
            {
                FileNameFilterMethod.Contains => (path, exec, provider) => path.ToString(provider).Contains(exec),
                FileNameFilterMethod.Exact => (path, exec, provider) => path.ToString(provider).Equals(exec),
                FileNameFilterMethod.StartWith => (path, exec, provider) => path.ToString(provider).StartsWith(exec),
                _ => (path, exec, provider) => path.ToString(provider).Contains(exec),
            };

            FFprobePath = files.FirstOrDefault(x => compareMethod(x.Name, _ffprobeExecutableName, FormatProvider) && IsExecutable(x.FullName))?.FullName;
            FFmpegPath = files.FirstOrDefault(x => compareMethod(x.Name, _ffmpegExecutableName, FormatProvider) && IsExecutable(x.FullName))?.FullName;

            ValidateExecutables();
            _lastExecutablePath = ExecutablesPath;
            return;
        }

        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly != null)
        {
            var workingDirectory = Path.GetDirectoryName(entryAssembly.Location)!;

            FindProgramsFromPath(workingDirectory);

            if (IsPathsSet())
            {
                return;
            }
        }

        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];

        foreach (var path in paths)
        {
            FindProgramsFromPath(path);

            if (IsPathsSet())
            {
                break;
            }
        }

        ValidateExecutables();
    }

    [MemberNotNull(nameof(FFmpegPath))]
    [MemberNotNull(nameof(FFprobePath))]
    private void ValidateExecutables()
    {
        if (IsPathsSet())
        {
            return;
        }

        var ffmpegDir = string.IsNullOrWhiteSpace(ExecutablesPath) ? string.Empty : string.Format(ExecutablesPath + " or ");
        var exceptionMessage = $"Cannot find FFmpeg in {ffmpegDir}PATH. This package needs installed FFmpeg. Please add it to your PATH variable or specify path to DIRECTORY with FFmpeg executables in {nameof(FFmpeg)}.{nameof(ExecutablesPath)}";

        throw new FFmpegNotFoundException(exceptionMessage);
    }

    private static bool IsExecutable(string file, OperatingSystemProvider? systemProvider = null, OperatingSystemArchitectureProvider? architectureProvider = null)
    {
        systemProvider ??= new OperatingSystemProvider();
        architectureProvider ??= new OperatingSystemArchitectureProvider();

        try
        {
            using var fileStream = File.OpenRead(file);

            var magicNumber = new byte[4];
            var appMagicNumber = new byte[4];
            fileStream.ReadExactly(magicNumber, offset: 0, count: 4);

            switch (systemProvider.GetOperatingSystem())
            {
                case OperatingSystem.Windows:
                    return magicNumber[0] == 0x4D && magicNumber[1] == 0x5A;
                case OperatingSystem.Osx:
                    return magicNumber[0] == 0xCE && magicNumber[1] == 0xFA && magicNumber[2] == 0xED && magicNumber[3] == 0xFE;
                case OperatingSystem.Linux:
                    var architecture = architectureProvider.GetArchitecture();

                    if (architecture is OperatingSystemArchitecture.X86 or OperatingSystemArchitecture.X64)
                    {
                        return magicNumber[0] == 0x7F && magicNumber[1] == 0x45 && magicNumber[2] == 0x4C && magicNumber[3] == 0x46;
                    }

                    fileStream.Seek(offset: 0x30, SeekOrigin.Begin);
                    fileStream.ReadExactly(appMagicNumber, offset: 0, count: 4);
                    return appMagicNumber[0] == 0x50 && appMagicNumber[1] == 0x4B && appMagicNumber[2] == 0x03 && appMagicNumber[3] == 0x04;
            }
        }
        catch (Exception)
        {
            //??
        }

        return false;
    }

    [MemberNotNullWhen(returnValue: true, nameof(FFmpegPath), nameof(FFprobePath))]
    private bool IsPathsSet()
    {
        return !string.IsNullOrWhiteSpace(FFmpegPath) && !string.IsNullOrWhiteSpace(FFprobePath);
    }

    private void FindProgramsFromPath(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        IEnumerable<FileInfo> files = new DirectoryInfo(path).GetFiles();

        FFprobePath = GetFullName(files, _ffprobeExecutableName);
        FFmpegPath = GetFullName(files, _ffmpegExecutableName);
    }

    internal static string? GetFullName(IEnumerable<FileInfo> files, string fileName)
    {
        return files.FirstOrDefault(x => x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase) || x.Name.Equals($"{fileName}.exe", StringComparison.InvariantCultureIgnoreCase))?.FullName;
    }

    /// <summary>
    ///     Run conversion
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <param name="processPath">FilePath to executable (FFmpeg, ffprobe)</param>
    /// <param name="priority">Process priority to run executables</param>
    /// <param name="standardInput">Should redirect standard input</param>
    /// <param name="standardOutput">Should redirect standard output</param>
    /// <param name="standardError">Should redirect standard error</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ObjectDisposedException"></exception>
    protected Process RunProcess
    (
        string args,
        string processPath,
        ProcessPriorityClass? priority,
        bool standardInput = false,
        bool standardOutput = false,
        bool standardError = false
    )
    {
        var process = new Process
                      {
                          StartInfo =
                          {
                              FileName = processPath,
                              Arguments = args,
                              UseShellExecute = false,
                              CreateNoWindow = true,
                              RedirectStandardInput = standardInput,
                              RedirectStandardOutput = standardOutput,
                              RedirectStandardError = standardError,
                          },
                          EnableRaisingEvents = true,
                      };

        process.Start();

        try
        {
            process.PriorityClass = priority ?? Process.GetCurrentProcess().PriorityClass;
        }
        catch (Exception)
        {
            // ignored
        }

        return process;
    }
}
