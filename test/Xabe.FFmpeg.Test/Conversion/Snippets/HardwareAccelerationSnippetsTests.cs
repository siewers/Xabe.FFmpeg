namespace Xabe.FFmpeg.Test;

public class HardwareAcceleration(StorageFixture storageFixture)
    : IClassFixture<StorageFixture>
{
    private readonly CancellationToken _testCancellationToken = TestContext.Current.CancellationToken;

    [RunnableInDebugOnly]
    public async Task ConversionWithHardware()
    {
        var output = storageFixture.GetTempFileName(FileExtensions.Mp4);
        await FFmpeg.Conversions.FromSnippet
                    .ConvertWithHardware(Resources.MkvWithAudio, output.FullName, HardwareAccelerator.cuvid, VideoCodec.h264_cuvid, VideoCodec.h264_nvenc, cancellationToken: _testCancellationToken)
                    .StartConversion(cancellationToken: _testCancellationToken);

        var mediaInfo = await FFmpeg.GetMediaInfo(output, _testCancellationToken);
        Assert.InRange(mediaInfo.Duration, 9.Seconds(), 11.Seconds());
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
