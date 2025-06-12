using Xabe.FFmpeg.Test.Common.Fixtures;

namespace Xabe.FFmpeg.Test;

public class HardwareAcceleration(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    [RunnableInDebugOnly]
    public async Task ConversionWithHardware()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        _ = await (await FFmpeg.Conversions.FromSnippet.ConvertWithHardware(Resources.MkvWithAudio, output.FullName, HardwareAccelerator.cuvid, VideoCodec.h264_cuvid, VideoCodec.h264_nvenc)).Start();

        var mediaInfo = await FFmpeg.GetMediaInfo(output);
        Assert.InRange(mediaInfo.Duration, TimeSpan.FromSeconds(9), TimeSpan.FromSeconds(11));
        Assert.Single(mediaInfo.VideoStreams);
        Assert.Single(mediaInfo.AudioStreams);
        var audioStream = mediaInfo.AudioStreams.First();
        var videoStream = mediaInfo.VideoStreams.First();
        Assert.NotNull(videoStream);
        Assert.NotNull(audioStream);
        Assert.Equal("h264", videoStream.Codec);
        Assert.Equal("aac", audioStream.Codec);
    }
}
