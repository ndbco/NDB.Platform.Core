using FluentAssertions;
using NDB.Platform.Kit.Format;
using Xunit;

namespace NDB.Platform.Tests.Kit.Format;

public sealed class DateTimeFormatTests
{
    [Fact]
    public void FormatTanggalIndonesiaLong_ShouldReturnIndonesianFormat()
    {
        var dt = new DateTime(2026, 5, 20);
        DateTimeFormat.FormatTanggalIndonesiaLong(dt).Should().Be("20 Mei 2026");
    }

    [Theory]
    [InlineData(1, "Januari")]
    [InlineData(12, "Desember")]
    [InlineData(7, "Juli")]
    public void FormatTanggalIndonesiaLong_ShouldUseIndonesianMonthNames(int month, string monthName)
    {
        var dt = new DateTime(2026, month, 1);
        DateTimeFormat.FormatTanggalIndonesiaLong(dt).Should().Contain(monthName);
    }

    [Fact]
    public void FormatTanggalIndonesiaShort_ShouldReturnCorrectFormat()
    {
        var dt = new DateTime(2026, 5, 20);
        DateTimeFormat.FormatTanggalIndonesiaShort(dt).Should().Be("20/05/2026");
    }

    [Fact]
    public void FormatJam_ShouldReturn24HourFormat()
    {
        var dt = new DateTime(2026, 1, 1, 14, 30, 0);
        DateTimeFormat.FormatJam(dt).Should().Be("14:30:00");
    }

    [Fact]
    public void FormatRelative_JustNow_ShouldReturnBaruSaja()
    {
        var now = DateTime.Now;
        var recent = now.AddSeconds(-30);
        DateTimeFormat.FormatRelative(recent, now).Should().Be("baru saja");
    }

    [Fact]
    public void FormatRelative_Hours_ShouldReturnJamLalu()
    {
        var now = DateTime.Now;
        var past = now.AddHours(-3);
        DateTimeFormat.FormatRelative(past, now).Should().Be("3 jam lalu");
    }

    [Fact]
    public void FormatRelative_Yesterday_ShouldReturnKemarin()
    {
        var now = DateTime.Now;
        var yesterday = now.AddHours(-25);
        DateTimeFormat.FormatRelative(yesterday, now).Should().Be("kemarin");
    }
}
