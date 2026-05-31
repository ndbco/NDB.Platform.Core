using FluentAssertions;
using NDB.Platform.Kit.Caching;
using Xunit;

namespace NDB.Platform.Tests.Kit.Caching;

public sealed class CacheKeyBuilderTests
{
    [Fact]
    public void For_EntityOnly_ShouldReturnEntityName()
    {
        var key = CacheKeyBuilder.For("role");
        key.Should().Be("role");
    }

    [Fact]
    public void For_EntityWithSingleKey_ShouldReturnColonSeparated()
    {
        var key = CacheKeyBuilder.For("role", "org-123");
        key.Should().Be("role:org-123");
    }

    [Fact]
    public void For_EntityWithMultipleKeys_ShouldReturnAllParts()
    {
        var key = CacheKeyBuilder.For("user", "org-1", "dept-2");
        key.Should().Be("user:org-1:dept-2");
    }

    [Fact]
    public void For_EntityWithGuidKey_ShouldStringifyGuid()
    {
        var orgId = new Guid("12345678-1234-1234-1234-123456789012");
        var key = CacheKeyBuilder.For("role", orgId);
        key.Should().Be($"role:{orgId}");
    }

    [Fact]
    public void For_EntityWithNullKey_ShouldUseNullLiteral()
    {
        object? nullKey = null;
        var key = CacheKeyBuilder.For("page", nullKey);
        key.Should().Be("page:null");
    }

    [Fact]
    public void For_EntityWithZeroKeys_ShouldNotAddColon()
    {
        var key = CacheKeyBuilder.For("permission");
        key.Should().NotContain(":");
    }
}
