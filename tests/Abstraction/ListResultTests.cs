using FluentAssertions;
using NDB.Platform.Abstraction;
using Xunit;

namespace NDB.Platform.Tests.Abstraction;

public sealed class ListResultTests
{
    [Fact]
    public void Success_ShouldContainItemsAndSuccessStatus()
    {
        var r = ListResult<string>.Success(new[] { "a", "b", "c" });
        r.IsSuccess.Should().BeTrue();
        r.Status.Should().Be(ResultStatus.Success);
        r.Items.Should().HaveCount(3);
        r.TotalCount.Should().Be(3);
    }

    [Fact]
    public void Success_WithExplicitTotalCount_ShouldUseTotalCount()
    {
        var r = ListResult<int>.Success(new[] { 1, 2 }, totalCount: 100);
        r.Items.Should().HaveCount(2);
        r.TotalCount.Should().Be(100);
    }

    [Fact]
    public void NotFound_ShouldHaveNotFoundStatus()
    {
        var r = ListResult<string>.NotFound("nothing here");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.NotFound);
        r.Items.Should().BeEmpty();
    }

    [Fact]
    public void Error_ShouldHaveErrorStatus()
    {
        var r = ListResult<string>.Error("oops");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void Unauthorized_ShouldHaveUnauthorizedStatus()
    {
        var r = ListResult<int>.Unauthorized();
        r.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public void Conflict_ShouldHaveConflictStatus()
    {
        var r = ListResult<bool>.Conflict("conflict");
        r.Status.Should().Be(ResultStatus.Conflict);
    }
}
