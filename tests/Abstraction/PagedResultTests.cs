using FluentAssertions;
using NDB.Platform.Abstraction;
using Xunit;

namespace NDB.Platform.Tests.Abstraction;

public sealed class PagedResultTests
{
    [Fact]
    public void Success_ShouldContainItemsAndPageInfo()
    {
        var r = PagedResult<string>.Success(new[] { "a", "b" }, page: 2, pageSize: 10, totalItems: 25);
        r.IsSuccess.Should().BeTrue();
        r.Items.Should().HaveCount(2);
        r.PageInfo.Page.Should().Be(2);
        r.PageInfo.PageSize.Should().Be(10);
        r.PageInfo.TotalItems.Should().Be(25);
        r.PageInfo.TotalPages.Should().Be(3);
    }

    [Fact]
    public void NotFound_ShouldHaveEmptyPageInfo()
    {
        var r = PagedResult<string>.NotFound();
        r.IsSuccess.Should().BeFalse();
        r.Items.Should().BeEmpty();
        r.PageInfo.Page.Should().Be(0);
        r.PageInfo.PageSize.Should().Be(0);
        r.PageInfo.TotalItems.Should().Be(0);
    }

    [Fact]
    public void Error_ShouldHaveErrorStatus()
    {
        var r = PagedResult<int>.Error("db error");
        r.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void Unauthorized_ShouldHaveCorrectStatus()
    {
        var r = PagedResult<string>.Unauthorized();
        r.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public void Forbidden_ShouldHaveCorrectStatus()
    {
        var r = PagedResult<bool>.Forbidden("no access");
        r.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public void Conflict_ShouldHaveCorrectStatus()
    {
        var r = PagedResult<string>.Conflict();
        r.Status.Should().Be(ResultStatus.Conflict);
    }
}
