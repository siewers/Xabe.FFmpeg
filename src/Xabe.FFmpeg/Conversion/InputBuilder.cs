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
    public Func<string, string> PrepareInputFiles(List<string> files, out string directory)
    {
        var directoryGuid = Guid.NewGuid();
        var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), directoryGuid.ToString()));
        directory = directoryInfo.FullName;

        for (var i = 0; i < files.Count; i++)
        {
            var destinationPath = Path.Combine(directory, BuildFileName(i + 1, Path.GetExtension(files[i])));

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(files[i], destinationPath);
            FileList.Add(new FileInfo(destinationPath));
        }

        return number => $" -i {Path.Combine(directoryInfo.FullName, "img" + number + FileList[0].Extension)} ";
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
