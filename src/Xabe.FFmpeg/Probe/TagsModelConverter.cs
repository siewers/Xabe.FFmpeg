namespace Xabe.FFmpeg.Probe;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models;

internal sealed class TagsModelConverter : JsonConverter<TagsModel>
{
    public override TagsModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonTags = JsonElement.ParseValue(ref reader);
        return new TagsModel(jsonTags);
    }

    public override void Write(Utf8JsonWriter writer, TagsModel value, JsonSerializerOptions options)
    {
    }
}
