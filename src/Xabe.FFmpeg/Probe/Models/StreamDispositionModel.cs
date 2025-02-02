namespace Xabe.FFmpeg;

using System;
using System.Collections.Generic;
using System.Text.Json;

internal sealed class StreamDispositionModel : Dictionary<string, bool>
{
    private StreamDispositionModel(JsonElement jsonDisposition)
        : base(StringComparer.OrdinalIgnoreCase)
    {
        foreach (var entry in jsonDisposition.EnumerateObject())
        {
            switch (entry.Name.ToLowerInvariant())
            {
                case "default":
                    IsDefault = entry.Value.GetInt32() == 1;
                    break;
                case "forced":
                    IsForced = entry.Value.GetInt32() == 1;
                    break;
                default:
                    Add(entry.Name, entry.Value.GetInt32() == 1);
                    break;
            }
        }
    }

    public bool? IsDefault { get; }

    public bool? IsForced { get; }

    public static StreamDispositionModel CreateInstance(JsonElement jsonDisposition)
    {
        return new StreamDispositionModel(jsonDisposition);
    }
}
