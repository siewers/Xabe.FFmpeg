using System.Text.Json.Serialization;

namespace Xabe.FFmpeg;

    internal sealed class VideoStreamModel : StreamModelBase
    {
        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("coded_height")]
        public int CodedHeight { get; set; }

        [JsonPropertyName("coded_width")]
        public int CodedWidth { get; set; }
    }
