namespace Xabe.FFmpeg.Test;

using System.IO;
using System.Linq;
using Xunit;

public class InputBuilderTests
{
    [Fact]
    public void PrepareInputFilesTest()
    {
        var files = Directory.EnumerateFiles(Resources.Images).ToList();
        var builder = new InputBuilder();

        builder.PrepareInputFiles(files, out var directory);
        var preparedFiles = Directory.EnumerateFiles(directory).ToList();

        Assert.Equal(12, builder.FileList.Count);
        Assert.Equal(builder.FileList.Count, preparedFiles.Count);
    }
}
