using FluentAssertions;
using NDB.Platform.Kit.Identifiers;
using Xunit;

namespace NDB.Platform.Tests.Kit.Identifiers;

public sealed class IdGeneratorTests
{
    [Fact]
    public void NewSnowflake_ShouldReturnPositiveLong()
    {
        var id = IdGenerator.NewSnowflake();
        id.Should().BePositive();
    }

    [Fact]
    public void NewSnowflake_TwoIds_ShouldBeUnique()
    {
        var id1 = IdGenerator.NewSnowflake();
        var id2 = IdGenerator.NewSnowflake();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void NewSnowflake_ShouldBeMonotonicallyIncreasing()
    {
        var ids = Enumerable.Range(0, 10).Select(_ => IdGenerator.NewSnowflake()).ToList();
        ids.Should().BeInAscendingOrder();
    }

    [Fact]
    public void NewUlid_ShouldReturnNonDefaultUlid()
    {
        var ulid = IdGenerator.NewUlid();
        ulid.Should().NotBe(default(Ulid));
    }

    [Fact]
    public void NewUlid_TwoIds_ShouldBeUnique()
    {
        var u1 = IdGenerator.NewUlid();
        var u2 = IdGenerator.NewUlid();
        u1.Should().NotBe(u2);
    }

    [Fact]
    public void NewNanoId_DefaultSize_ShouldReturn21Chars()
    {
        var id = IdGenerator.NewNanoId();
        id.Should().HaveLength(21);
    }

    [Fact]
    public void NewNanoId_CustomSize_ShouldReturnCorrectLength()
    {
        var id = IdGenerator.NewNanoId(10);
        id.Should().HaveLength(10);
    }

    [Fact]
    public void NewNanoId_TwoIds_ShouldBeUnique()
    {
        var id1 = IdGenerator.NewNanoId();
        var id2 = IdGenerator.NewNanoId();
        id1.Should().NotBe(id2);
    }
}
