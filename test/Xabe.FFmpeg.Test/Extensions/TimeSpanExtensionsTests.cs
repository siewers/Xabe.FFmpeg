namespace Xabe.FFmpeg.Test.Extensions;

using Probe;

public sealed class TimeSpanParserTests
{
    public static TheoryData<string, TimeSpan> ParseTimeSpanTestCases =>
        new()
        {
            { "01:29:43.253000000", 1.Hours().And(29.Minutes().And(43.Seconds().And(253.Milliseconds()))) },
            { "5688.704000", 1.Hours().And(34.Minutes().And(48.Seconds().And(704.Milliseconds()))) },
        };

    [Theory]
    [MemberData(nameof(ParseTimeSpanTestCases))]
    public void ParseTimeSpan_ReturnsExpectedResult(string input, TimeSpan expected)
    {
        var actual = TimeSpanParser.Parse(input);

        actual.Should().Be(expected);
    }
}
