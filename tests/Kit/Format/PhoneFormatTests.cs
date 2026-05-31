using FluentAssertions;
using NDB.Platform.Kit.Format;
using Xunit;

namespace NDB.Platform.Tests.Kit.Format;

public sealed class PhoneFormatTests
{
    [Theory]
    [InlineData("08123456789", "+628123456789")]
    [InlineData("+628123456789", "+628123456789")]
    [InlineData("628123456789", "+628123456789")]
    public void NormalizeIndonesianPhone_ShouldReturnE164Format(string input, string expected)
    {
        PhoneFormat.NormalizeIndonesianPhone(input).Should().Be(expected);
    }

    [Fact]
    public void NormalizeIndonesianPhone_EmptyString_ShouldReturnEmpty()
    {
        PhoneFormat.NormalizeIndonesianPhone("").Should().BeEmpty();
    }

    [Fact]
    public void NormalizeIndonesianPhone_WhitespaceString_ShouldReturnEmpty()
    {
        PhoneFormat.NormalizeIndonesianPhone("   ").Should().BeEmpty();
    }

    [Theory]
    [InlineData("08123456789", "0812-3456-789")]
    [InlineData("081234567890", "0812-3456-7890")]
    public void FormatDisplay_ShouldFormatWithDashes(string input, string expected)
    {
        PhoneFormat.FormatDisplay(input).Should().Be(expected);
    }
}
