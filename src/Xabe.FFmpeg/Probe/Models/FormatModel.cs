namespace Xabe.FFmpeg.Probe.Models;

using System.Text.Json;

internal sealed class FormatModel : Dictionary<string, object>
{
    public FormatModel(JsonElement element)
        : base(StringComparer.OrdinalIgnoreCase)
    {
        foreach (var entry in element.EnumerateObject())
        {
            switch (entry.Name.ToLowerInvariant())
            {
                case "filename":
                    FileName = entry.Value.GetString()!.Escape();
                    break;
                case "size":
                    Size = long.Parse(entry.Value.GetString()!);
                    break;
                case "bit_rate":
                    Bitrate = long.Parse(entry.Value.GetString()!);
                    break;
                case "duration":
                    Duration = entry.Value.GetTimeSpan()!.Value;
                    break;
                case "tags":
                    Tags = new TagsModel(entry.Value);
                    break;
                default:
                    Add(entry.Name, entry.Value);
                    break;
            }
        }
    }

    public string FileName { get; } = null!;

    public long Size { get; }

    public long? Bitrate { get; }

    public TimeSpan Duration { get; }

    public TagsModel Tags { get; } = new();
}
