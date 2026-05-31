using FluentAssertions;
using NDB.Platform.Abstraction;
using Xunit;

namespace NDB.Platform.Tests.Abstraction;

public sealed class ResultOfTTests
{
    [Fact]
    public void Success_ShouldContainDataAndSuccessStatus()
    {
        var r = Result<string>.Success("hello");
        r.IsSuccess.Should().BeTrue();
        r.Status.Should().Be(ResultStatus.Success);
        r.Data.Should().Be("hello");
        r.Errors.Should().BeEmpty();
        r.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void NotFound_ShouldHaveNullData()
    {
        var r = Result<string>.NotFound("nope");
        r.IsSuccess.Should().BeFalse();
        r.Data.Should().BeNull();
        r.Message.Should().Be("nope");
    }

    [Fact]
    public void BadRequest_WithErrors_ShouldContainErrors()
    {
        var r = Result<int>.BadRequest(new[] { "e1", "e2" });
        r.Status.Should().Be(ResultStatus.BadRequest);
        r.Errors.Should().HaveCount(2);
        r.Data.Should().Be(0);
    }

    [Fact]
    public void Unauthorized_ShouldHaveCorrectStatus()
    {
        var r = Result<bool>.Unauthorized();
        r.Status.Should().Be(ResultStatus.Unauthorized);
        r.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Forbidden_ShouldHaveCorrectStatus()
    {
        var r = Result<double>.Forbidden("no access");
        r.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public void Error_ShouldHaveErrorStatus()
    {
        var r = Result<object>.Error("something went wrong");
        r.Status.Should().Be(ResultStatus.Error);
        r.Message.Should().Be("something went wrong");
    }

    [Fact]
    public void Conflict_ShouldHaveConflictStatus()
    {
        var r = Result<Guid>.Conflict("already exists");
        r.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public void Validation_ShouldHaveValidationErrorsAndBadRequestStatus()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = new[] { "Name is required" },
            ["Email"] = new[] { "Email format invalid", "Email too long" }
        };
        var r = Result<string>.Validation(errors);
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.BadRequest);
        r.ValidationErrors.Should().NotBeNull();
        r.ValidationErrors!["Name"].Should().ContainSingle("Name is required");
        r.ValidationErrors["Email"].Should().HaveCount(2);
        r.Errors.Should().HaveCount(3); // 1+2 flat errors
    }

    [Fact]
    public void Success_WithComplexType_ShouldReturnData()
    {
        var dto = new TestDto { Id = 1, Name = "Test" };
        var r = Result<TestDto>.Success(dto);
        r.IsSuccess.Should().BeTrue();
        r.Data!.Id.Should().Be(1);
        r.Data.Name.Should().Be("Test");
    }

    [Fact]
    public void BadRequest_WithSingleMessage_ShouldWork()
    {
        var r = Result<int>.BadRequest("bad!");
        r.Status.Should().Be(ResultStatus.BadRequest);
        r.Message.Should().Be("bad!");
        r.Errors.Should().ContainSingle();
    }

    private sealed class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
