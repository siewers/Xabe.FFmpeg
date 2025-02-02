namespace Xabe.FFmpeg;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class TagsModelConverter : JsonConverter<TagsModel>
{
    public override TagsModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonTags = JsonElement.ParseValue(ref reader);
        return TagsModel.Create(jsonTags);
    }

    public override void Write(Utf8JsonWriter writer, TagsModel value, JsonSerializerOptions options)
    {
    }
}
