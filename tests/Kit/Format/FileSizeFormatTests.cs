using FluentAssertions;
using NDB.Platform.Kit.Format;
using Xunit;

namespace NDB.Platform.Tests.Kit.Format;

public sealed class FileSizeFormatTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(500, "500 B")]
    [InlineData(1024, "1,0 KB")]
    [InlineData(1048576, "1,0 MB")]
    [InlineData(1073741824, "1,0 GB")]
    public void FormatBytes_ShouldFormatCorrectly(long bytes, string expected)
    {
        FileSizeFormat.FormatBytes(bytes).Should().Be(expected);
    }

    [Fact]
    public void FormatBytes_LargeFile_ShouldFormatCorrectly()
    {
        // 1.5 MB
        var result = FileSizeFormat.FormatBytes(1572864);
        result.Should().Be("1,5 MB");
    }

    [Fact]
    public void ParseToBytes_KB_ShouldReturnCorrectBytes()
    {
        var bytes = FileSizeFormat.ParseToBytes("1 KB");
        bytes.Should().Be(1024);
    }

    [Fact]
    public void ParseToBytes_MB_ShouldReturnCorrectBytes()
    {
        var bytes = FileSizeFormat.ParseToBytes("1 MB");
        bytes.Should().Be(1048576);
    }

    [Fact]
    public void ParseToBytes_EmptyString_ShouldReturnZero()
    {
        FileSizeFormat.ParseToBytes("").Should().Be(0);
    }
}
