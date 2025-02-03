using System.Collections.Generic;
using System.IO;
using Xabe.FFmpeg.Test.Common.Fixtures;
using Xunit;

namespace Xabe.FFmpeg.Test;

public class FFmpegExecutablesHelperTests(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    [Fact]
    public void TestSelectFFmpegPathForWindows()
    {
        const string expected = "ffmpeg.exe";

        var files = GetWindowsPathMocks();

        var path = FFmpeg.GetFullName(files, "ffmpeg");

        Assert.EndsWith(expected, path);
    }

    [Fact]
    public void TestSelectFFprobePathForWindows()
    {
        const string expected = "ffprobe.exe";

        var files = GetWindowsPathMocks();

        var path = FFmpeg.GetFullName(files, "ffprobe");

        Assert.EndsWith(expected, path);
    }

    [Fact]
    public void TestSelectFFmpegPathForLinux()
    {
        const string expected = "ffmpeg";

        var files = GetLinuxPathMocks();

        var path = FFmpeg.GetFullName(files, "ffmpeg");

        Assert.EndsWith(expected, path);
    }

    [Fact]
    public void TestSelectFFprobePathForLinux()
    {
        const string expected = "ffprobe";

        var files = GetLinuxPathMocks();

        var path = FFmpeg.GetFullName(files, "ffprobe");

        Assert.EndsWith(expected, path);
    }

    private IEnumerable<FileInfo> GetWindowsPathMocks()
    {
        var tmpDir = storageFixture.GetTempDirectory();

        File.Create(Path.Combine(tmpDir, "ffmpeg.exe"));
        File.Create(Path.Combine(tmpDir, "FFmpeg.AutoGen.dll"));
        File.Create(Path.Combine(tmpDir, "ffprobe.exe"));
        return new DirectoryInfo(tmpDir).EnumerateFiles();
    }

    private IEnumerable<FileInfo> GetLinuxPathMocks()
    {
        var tmpDir = storageFixture.GetTempDirectory();

        File.Create(Path.Combine(tmpDir, "ffmpeg"));
        File.Create(Path.Combine(tmpDir, "FFmpeg.AutoGen.dll"));
        File.Create(Path.Combine(tmpDir, "ffprobe"));
        return new DirectoryInfo(tmpDir).EnumerateFiles();
    }
}
