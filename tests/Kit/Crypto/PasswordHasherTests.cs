using FluentAssertions;
using NDB.Platform.Kit.Crypto;
using Xunit;

namespace NDB.Platform.Tests.Kit.Crypto;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_ShouldProduceNonEmptyHash()
    {
        var hash = PasswordHasher.Hash("password123");
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2a$");
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        var plain = "mySecret!";
        var hash = PasswordHasher.Hash(plain);
        PasswordHasher.Verify(plain, hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ShouldReturnFalse()
    {
        var hash = PasswordHasher.Hash("correctPassword");
        PasswordHasher.Verify("wrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePassword_ShouldProduceDifferentHashes()
    {
        var h1 = PasswordHasher.Hash("same");
        var h2 = PasswordHasher.Hash("same");
        h1.Should().NotBe(h2); // BCrypt uses random salt
    }
}
