using FluentAssertions;
using NDB.Platform.Kit.Identifiers;
using Xunit;

namespace NDB.Platform.Tests.Kit.Identifiers;

public sealed class CorrelationIdTests
{
    [Fact]
    public void GetOrCreate_ShouldReturnNonEmptyString()
    {
        // Reset state
        CorrelationId.Value = null;
        var id = CorrelationId.GetOrCreate();
        id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetOrCreate_CalledTwice_ShouldReturnSameId()
    {
        CorrelationId.Value = null;
        var id1 = CorrelationId.GetOrCreate();
        var id2 = CorrelationId.GetOrCreate();
        id1.Should().Be(id2);
    }

    [Fact]
    public void Value_SetManually_ShouldBeReturnedByGetOrCreate()
    {
        CorrelationId.Value = "custom-id-123";
        var id = CorrelationId.GetOrCreate();
        id.Should().Be("custom-id-123");
        // Cleanup
        CorrelationId.Value = null;
    }

    [Fact]
    public void HeaderName_ShouldBeCorrect()
    {
        CorrelationId.HeaderName.Should().Be("X-Correlation-ID");
    }
}
