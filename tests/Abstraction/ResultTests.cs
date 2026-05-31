using FluentAssertions;
using NDB.Platform.Abstraction;
using Xunit;

namespace NDB.Platform.Tests.Abstraction;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldHaveSuccessStatus()
    {
        var r = Result.Success();
        r.IsSuccess.Should().BeTrue();
        r.Status.Should().Be(ResultStatus.Success);
        r.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithMessage_ShouldSetMessage()
    {
        var r = Result.Success("Operasi berhasil");
        r.IsSuccess.Should().BeTrue();
        r.Message.Should().Be("Operasi berhasil");
    }

    [Fact]
    public void NotFound_ShouldHaveNotFoundStatus()
    {
        var r = Result.NotFound("tidak ditemukan");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.NotFound);
        r.Message.Should().Be("tidak ditemukan");
    }

    [Fact]
    public void NotFound_WithoutMessage_ShouldHaveNullMessage()
    {
        var r = Result.NotFound();
        r.Status.Should().Be(ResultStatus.NotFound);
        r.Message.Should().BeNull();
    }

    [Fact]
    public void BadRequest_WithErrors_ShouldContainAllErrors()
    {
        var r = Result.BadRequest(new[] { "error1", "error2", "error3" });
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.BadRequest);
        r.Errors.Should().HaveCount(3);
        r.Errors.Should().Contain("error1");
        r.Errors.Should().Contain("error3");
    }

    [Fact]
    public void BadRequest_WithSingleMessage_ShouldHaveMessageAndError()
    {
        var r = Result.BadRequest("invalid input");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.BadRequest);
        r.Message.Should().Be("invalid input");
        r.Errors.Should().ContainSingle();
    }

    [Fact]
    public void Unauthorized_ShouldHaveUnauthorizedStatus()
    {
        var r = Result.Unauthorized("login required");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Unauthorized);
        r.Message.Should().Be("login required");
    }

    [Fact]
    public void Forbidden_ShouldHaveForbiddenStatus()
    {
        var r = Result.Forbidden();
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Forbidden);
    }

    [Fact]
    public void Error_ShouldHaveErrorStatus()
    {
        var r = Result.Error("internal error");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Error);
        r.Message.Should().Be("internal error");
    }

    [Fact]
    public void Conflict_ShouldHaveConflictStatus()
    {
        var r = Result.Conflict("data conflict");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Conflict);
        r.Message.Should().Be("data conflict");
    }

    [Fact]
    public void GenericSuccess_ShouldContainData()
    {
        var r = Result.Success(42);
        r.IsSuccess.Should().BeTrue();
        r.Data.Should().Be(42);
        r.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public void GenericNotFound_ShouldHaveCorrectStatus()
    {
        var r = Result.NotFound<string>("item not found");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.NotFound);
        r.Data.Should().BeNull();
    }

    [Fact]
    public void GenericBadRequest_ShouldContainErrors()
    {
        var r = Result.BadRequest<int>(new[] { "err1", "err2" });
        r.IsSuccess.Should().BeFalse();
        r.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void GenericError_ShouldHaveErrorStatus()
    {
        var r = Result.Error<bool>("kaboom");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Error);
    }

    [Fact]
    public void GenericConflict_ShouldHaveConflictStatus()
    {
        var r = Result.Conflict<string>("duplicate");
        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public void Errors_ShouldBeImmutable()
    {
        var r = Result.BadRequest(new[] { "e1" });
        var errors = r.Errors;
        errors.Should().HaveCount(1);
    }

    // ── FIX 1 (C-11): Result.Error(string, Exception?) + Result.Validation() non-generic ──

    [Fact]
    public void Error_WithException_Should_Set_Exception_Property()
    {
        var ex = new InvalidOperationException("test exception");
        var r = Result.Error("internal error", ex);

        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Error);
        r.Message.Should().Be("internal error");
        r.Exception.Should().BeSameAs(ex);
    }

    [Fact]
    public void Error_WithNullException_Should_Have_Null_Exception()
    {
        var r = Result.Error("internal error", null);

        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Error);
        r.Exception.Should().BeNull();
    }

    [Fact]
    public void Error_Without_Exception_Param_Should_Still_Compile_And_Work()
    {
        // backward compat: original overload tanpa exception param
        var r = Result.Error("msg");
        r.Status.Should().Be(ResultStatus.Error);
        r.Exception.Should().BeNull();
    }

    [Fact]
    public void Validation_NonGeneric_Should_Set_ValidationErrors_And_Errors()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name wajib diisi", "Name min 3 karakter"],
            ["Email"] = ["Email tidak valid"]
        };

        var r = Result.Validation(errors);

        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.BadRequest);
        r.Message.Should().Be("Validation failed");
        r.ValidationErrors.Should().NotBeNull();
        r.ValidationErrors!.Should().ContainKey("Name");
        r.ValidationErrors!.Should().ContainKey("Email");
        r.Errors.Should().HaveCount(3); // flat: 2 + 1
    }

    [Fact]
    public void Validation_NonGeneric_With_CustomMessage_Should_Use_That_Message()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Field"] = ["Error"]
        };

        var r = Result.Validation(errors, "Custom validation message");

        r.Message.Should().Be("Custom validation message");
    }

    [Fact]
    public void GenericError_WithException_Should_Set_Exception_Property()
    {
        var ex = new ArgumentException("arg err");
        var r = Result.Error<int>("generic error", ex);

        r.IsSuccess.Should().BeFalse();
        r.Status.Should().Be(ResultStatus.Error);
        r.Exception.Should().BeSameAs(ex);
    }
}
