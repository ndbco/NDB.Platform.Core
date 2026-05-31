using FluentAssertions;
using NDB.Platform.Abstraction;
using Xunit;

namespace NDB.Platform.Tests.Abstraction;

public sealed class PageInfoTests
{
    [Fact]
    public void Create_ShouldCalculateTotalPagesCorrectly()
    {
        var pi = PageInfo.Create(1, 10, 25);
        pi.Page.Should().Be(1);
        pi.PageSize.Should().Be(10);
        pi.TotalItems.Should().Be(25);
        pi.TotalPages.Should().Be(3);
    }

    [Fact]
    public void Create_ExactPages_ShouldNotAddExtraPage()
    {
        var pi = PageInfo.Create(1, 10, 30);
        pi.TotalPages.Should().Be(3);
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ShouldBeFalse()
    {
        var pi = PageInfo.Create(1, 10, 50);
        pi.HasPreviousPage.Should().BeFalse();
        pi.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_OnLastPage_ShouldBeFalse()
    {
        var pi = PageInfo.Create(5, 10, 50);
        pi.HasNextPage.Should().BeFalse();
        pi.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void Empty_ShouldHaveZeroValues()
    {
        var pi = PageInfo.Empty;
        pi.Page.Should().Be(0);
        pi.TotalItems.Should().Be(0);
        pi.TotalPages.Should().Be(0);
        pi.HasPreviousPage.Should().BeFalse();
        pi.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Create_ZeroPageSize_ShouldHaveZeroTotalPages()
    {
        var pi = PageInfo.Create(1, 0, 100);
        pi.TotalPages.Should().Be(0);
    }
}
