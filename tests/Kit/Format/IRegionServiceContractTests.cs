using FluentAssertions;
using NDB.Platform.Kit.Format;
using NSubstitute;
using Xunit;

namespace NDB.Platform.Tests.Kit.Format;

/// <summary>
/// Contract tests untuk IRegionService — verifikasi interface dapat dimock dengan benar
/// dan return type sesuai ekspektasi.
/// </summary>
public sealed class IRegionServiceContractTests
{
    [Fact]
    public void FormatCurrency_MockReturnsExpected()
    {
        var svc = Substitute.For<IRegionService>();
        svc.FormatCurrency(1_000_000m, "IDR", "id-ID").Returns("Rp 1.000.000");

        var result = svc.FormatCurrency(1_000_000m, "IDR", "id-ID");

        result.Should().Be("Rp 1.000.000");
    }

    [Fact]
    public void FormatDate_MockReturnsExpected()
    {
        var svc = Substitute.For<IRegionService>();
        var dt = new DateTime(2026, 5, 31);
        svc.FormatDate(dt, "id-ID").Returns("31 Mei 2026");

        var result = svc.FormatDate(dt, "id-ID");

        result.Should().Be("31 Mei 2026");
    }

    [Fact]
    public void FormatPhone_MockReturnsE164()
    {
        var svc = Substitute.For<IRegionService>();
        svc.FormatPhone("0812345678").Returns("+6281234567");

        var result = svc.FormatPhone("0812345678");

        result.Should().StartWith("+");
        result.Should().Be("+6281234567");
    }

    [Fact]
    public void FormatCurrency_NullLocale_ShouldAllowNull()
    {
        var svc = Substitute.For<IRegionService>();
        svc.FormatCurrency(500m, "USD", null).Returns("$500.00");

        var result = svc.FormatCurrency(500m, "USD", null);

        result.Should().NotBeNullOrEmpty();
    }
}
