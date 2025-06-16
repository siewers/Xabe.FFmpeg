namespace Xabe.FFmpeg.Test;

internal static class Resources
{
    internal static readonly MediaLocation PngSample = GetResourceFilePath("watermark.png");
    internal static readonly MediaLocation Mp4WithAudio = GetResourceFilePath("input.mp4");
    internal static readonly MediaLocation Mp3 = GetResourceFilePath("audio.mp3");
    internal static readonly MediaLocation Mp4 = GetResourceFilePath("mute.mp4");
    internal static readonly MediaLocation MkvWithAudio = GetResourceFilePath("SampleVideo_360x240_1mb.mkv");
    internal static readonly MediaLocation MkvWithSubtitles = GetResourceFilePath("mkvWithSubtitles.mkv");
    internal static readonly MediaLocation MultipleStream = GetResourceFilePath("multipleStreamSample.mkv");
    internal static readonly MediaLocation TsWithAudio = GetResourceFilePath("sample.ts");
    internal static readonly MediaLocation FlvWithAudio = GetResourceFilePath("sample.flv");
    internal static readonly MediaLocation BunnyMp4 = GetResourceFilePath("bunny.mp4");
    internal static readonly MediaLocation SloMoMp4 = GetResourceFilePath("slomo.mp4");
    internal static readonly MediaLocation Dll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Xabe.FFmpeg.Test.dll");
    internal static readonly MediaLocation Images = GetResourceFilePath("Images");
    internal static readonly MediaLocation SubtitleSrt = GetResourceFilePath("sampleSrt.srt");
    internal static readonly MediaLocation FFbinariesInfo = GetResourceFilePath("ffbinaries.json");

    private static MediaLocation GetResourceFilePath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
    }
}
