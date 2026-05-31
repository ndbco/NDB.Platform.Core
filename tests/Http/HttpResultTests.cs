using FluentAssertions;
using NDB.Platform.Http;
using Xunit;

namespace NDB.Platform.Tests.Http;

public sealed class HttpResultTests
{
    [Fact]
    public void Ok_NonGeneric_ShouldHaveSucceededTrue()
    {
        var r = HttpResult.Ok(200);
        r.Succeeded.Should().BeTrue();
        r.StatusCode.Should().Be(200);
        r.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Fail_NonGeneric_ShouldHaveSucceededFalse()
    {
        var r = HttpResult.Fail(500, "Internal Server Error");
        r.Succeeded.Should().BeFalse();
        r.StatusCode.Should().Be(500);
        r.ErrorMessage.Should().Be("Internal Server Error");
    }

    [Fact]
    public void Ok_Generic_ShouldContainData()
    {
        var r = HttpResult<string>.Ok("hello", 200);
        r.Succeeded.Should().BeTrue();
        r.Data.Should().Be("hello");
        r.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Fail_Generic_ShouldHaveNullData()
    {
        var r = HttpResult<string>.Fail(404, "Not Found");
        r.Succeeded.Should().BeFalse();
        r.Data.Should().BeNull();
        r.StatusCode.Should().Be(404);
        r.ErrorMessage.Should().Be("Not Found");
    }

    [Fact]
    public void Ok_WithRaw_ShouldStoreRawResponse()
    {
        var raw = "{\"id\":1}";
        var r = HttpResult<object>.Ok(new object(), 200, raw);
        r.Raw.Should().Be(raw);
    }
}
