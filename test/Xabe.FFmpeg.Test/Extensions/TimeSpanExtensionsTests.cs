namespace Xabe.FFmpeg.Test.Extensions;

using System;
using Xunit;

public sealed class TimeSpanParserTests
{
    public static TheoryData<string, TimeSpan> ParseTimeSpanTestCases =>
        new()
        {
            { "01:29:43.253000000", new TimeSpan(0, 1, 29, 43, 253) },
            { "5688.704000", new TimeSpan(0, 1, 34, 48, 704) },
        };

    [Theory]
    [MemberData(nameof(ParseTimeSpanTestCases))]
    public void ParseTimeSpan_ReturnsExpectedResult(string input, TimeSpan expected)
    {
        var actual = TimeSpanParser.Parse(input);

        Assert.Equal(expected, actual);
    }
}
