namespace Xabe.FFmpeg.Probe;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

internal static class JsonDeserializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
                                                                          {
                                                                              PropertyNameCaseInsensitive = false,
                                                                              AllowOutOfOrderMetadataProperties = true,
                                                                              Converters =
                                                                              {
                                                                                  new TimeSpanConverter(),
                                                                                  new StreamDispositionModelConverter(),
                                                                                  new FormatModelConverter(),
                                                                                  new TagsModelConverter(),
                                                                              },
                                                                              TypeInfoResolver = new DefaultJsonTypeInfoResolver
                                                                                                 {
                                                                                                     Modifiers = { SetNumberHandlingModifier },
                                                                                                 },
                                                                          };

    private static void SetNumberHandlingModifier(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Type == typeof(int) ||
            jsonTypeInfo.Type == typeof(int?) ||
            jsonTypeInfo.Type == typeof(long) ||
            jsonTypeInfo.Type == typeof(long?) ||
            jsonTypeInfo.Type == typeof(double) ||
            jsonTypeInfo.Type == typeof(double?))
        {
            jsonTypeInfo.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        }
    }

    public static T? Deserialize<[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)] T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }

    private sealed class TimeSpanConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var timeSpan = reader.TokenType == JsonTokenType.Number
                ? TimeSpan.FromSeconds(reader.GetDouble())
                : TimeSpanParser.Parse(reader.GetString());

            return timeSpan;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
        }
    }
}
