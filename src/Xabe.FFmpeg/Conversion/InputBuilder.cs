namespace Xabe.FFmpeg;

/// <summary>
///     Default Implementation of the IInputBuilder Interface
/// </summary>
[PublicAPI]
public sealed class InputBuilder : IInputBuilder
{
    /// <inheritdoc />
    public List<FileInfo> FileList { get; } = [];

    /// <inheritdoc />
    public Func<string, MediaLocation> PrepareInputFiles(IEnumerable<MediaLocation> files, out string directory)
    {
        var filesArray = files.ToArray();
        var directoryGuid = Guid.NewGuid();
        var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), directoryGuid.ToString()));
        directoryInfo.Create();
        directory = directoryInfo.FullName;

        for (var i = 0; i < filesArray.Length; i++)
        {
            var destinationPath = Path.Combine(directoryInfo.FullName, BuildFileName(i + 1, Path.GetExtension(filesArray[i])));
            File.Copy(filesArray[i], destinationPath);
            FileList.Add(new FileInfo(destinationPath));
        }

        return number => $" -i {Path.Combine(directoryInfo.FullName, $"img{number}{FileList[0].Extension}")} ";
    }

    private static string BuildFileName(int fileIndex, string extension)
    {
        var name = "img_";

        name += fileIndex switch
        {
            < 10 => $"00{fileIndex}" + extension,
            < 100 => $"0{fileIndex}" + extension,
            _ => $"{fileIndex}" + extension,
        };

        return name;
    }
}
