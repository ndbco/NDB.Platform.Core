using FluentAssertions;
using NDB.Platform.Http.Resilience;
using Xunit;

namespace NDB.Platform.Tests.Http.Resilience;

public sealed class TokenResponseTests
{
    [Fact]
    public void TokenResponse_ShouldStoreValues()
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(15);
        var token = new TokenResponse("access-token", "refresh-token", expiry);

        token.AccessToken.Should().Be("access-token");
        token.RefreshToken.Should().Be("refresh-token");
        token.ExpiresAt.Should().Be(expiry);
    }

    [Fact]
    public void TokenResponse_ShouldSupportDeconstruct()
    {
        var expiry = DateTimeOffset.UtcNow;
        var (access, refresh, at) = new TokenResponse("a", "r", expiry);

        access.Should().Be("a");
        refresh.Should().Be("r");
        at.Should().Be(expiry);
    }

    [Fact]
    public void TokenResponse_EqualTokens_ShouldBeEqual()
    {
        var expiry = DateTimeOffset.UtcNow;
        var t1 = new TokenResponse("a", "r", expiry);
        var t2 = new TokenResponse("a", "r", expiry);
        t1.Should().Be(t2);
    }
}
