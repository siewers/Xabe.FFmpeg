namespace Xabe.FFmpeg.Test;

using Probe;

public class ProbeTests
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task StartWithCsvResultTest()
    {
        var result = await Probe.New()
                                .Start($"-loglevel error -skip_frame nokey -select_streams v:0 -show_entries frame=pkt_pts_time -of csv=print_section=0 {Resources.Mp4}", _cancellationToken);

        var values = result.Split('\n')
                           .Where(x => !string.IsNullOrEmpty(x));

        values.Should().HaveCount(3);
    }

    [Fact]
    public async Task StartWithStdOutputTest()
    {
        var result = await Probe.New()
                                .Start($"-loglevel error -skip_frame nokey -select_streams v:0 -show_entries frame=pkt_pts_time {Resources.Mp4}", _cancellationToken);

        result.Should().NotBeNullOrEmpty();
    }
}
