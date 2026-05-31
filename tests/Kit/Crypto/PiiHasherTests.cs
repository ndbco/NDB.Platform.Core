using FluentAssertions;
using NDB.Platform.Kit.Crypto;
using Xunit;

namespace NDB.Platform.Tests.Kit.Crypto;

public sealed class PiiHasherTests
{
    [Fact]
    public void Hash_ShouldBeDeterministic()
    {
        var h1 = PiiHasher.Hash("user@example.com");
        var h2 = PiiHasher.Hash("user@example.com");
        h1.Should().Be(h2);
    }

    [Fact]
    public void Hash_WithSalt_ShouldBeDeterministic()
    {
        var h1 = PiiHasher.Hash("08123456789", "tenant-abc");
        var h2 = PiiHasher.Hash("08123456789", "tenant-abc");
        h1.Should().Be(h2);
    }

    [Fact]
    public void Hash_DifferentSalt_ShouldProduceDifferentHash()
    {
        var h1 = PiiHasher.Hash("value", "salt-a");
        var h2 = PiiHasher.Hash("value", "salt-b");
        h1.Should().NotBe(h2);
    }

    [Fact]
    public void Hash_ShouldReturnLowercaseHex()
    {
        var hash = PiiHasher.Hash("test");
        hash.Should().MatchRegex("^[a-f0-9]+$");
    }

    [Fact]
    public void Hash_WithoutAndWithSalt_ShouldDiffer()
    {
        var withoutSalt = PiiHasher.Hash("value");
        var withSalt = PiiHasher.Hash("value", "salt");
        withoutSalt.Should().NotBe(withSalt);
    }
}
