using FluentAssertions;
using NDB.Platform.Kit.Format;
using Xunit;

namespace NDB.Platform.Tests.Kit.Format;

public sealed class NumberFormatTests
{
    [Theory]
    [InlineData(1234567.89, "Rp 1.234.567,89")]
    [InlineData(0, "Rp 0,00")]
    [InlineData(1000, "Rp 1.000,00")]
    public void FormatRupiah_ShouldFormatCorrectly(decimal amount, string expected)
    {
        NumberFormat.FormatRupiah(amount).Should().Be(expected);
    }

    [Theory]
    [InlineData(12.5, 2, "12,50%")]
    [InlineData(100, 0, "100%")]
    public void FormatPercent_ShouldFormatCorrectly(decimal value, int decimals, string expected)
    {
        NumberFormat.FormatPercent(value, decimals).Should().Be(expected);
    }

    [Theory]
    [InlineData(1234.5678, 2, "1.234,57")]
    [InlineData(1000000, 0, "1.000.000")]
    public void FormatDecimal_ShouldFormatCorrectly(decimal value, int decimals, string expected)
    {
        NumberFormat.FormatDecimal(value, decimals).Should().Be(expected);
    }

    [Theory]
    [InlineData(1234567L, "1.234.567")]
    [InlineData(0L, "0")]
    public void FormatThousands_ShouldFormatCorrectly(long value, string expected)
    {
        NumberFormat.FormatThousands(value).Should().Be(expected);
    }
}
