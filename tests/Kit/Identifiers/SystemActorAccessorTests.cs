using FluentAssertions;
using NDB.Platform.Abstraction;
using NDB.Platform.Kit.Identifiers;
using Xunit;

namespace NDB.Platform.Tests.Kit.Identifiers;

/// <summary>
/// Tests untuk SystemActorAccessor (FIX 4 — Sprint B C-06).
/// </summary>
public sealed class SystemActorAccessorTests
{
    [Fact]
    public void GetCurrent_ShouldReturn_SystemActor()
    {
        var accessor = new SystemActorAccessor();

        var actor = accessor.GetCurrent();

        actor.Actor.Should().Be("SYSTEM");
    }

    [Fact]
    public void GetCurrent_ShouldReturn_NullActorId()
    {
        var accessor = new SystemActorAccessor();

        var actor = accessor.GetCurrent();

        actor.ActorId.Should().BeNull();
        actor.Role.Should().BeNull();
    }

    [Fact]
    public void GetCurrent_ShouldAttach_CorrelationId_WhenSet()
    {
        // Arrange
        var correlationId = "test-correlation-id-1234";
        CorrelationId.Value = correlationId;

        var accessor = new SystemActorAccessor();

        try
        {
            // Act
            var actor = accessor.GetCurrent();

            // Assert
            actor.CorrelationId.Should().Be(correlationId);
        }
        finally
        {
            // Cleanup AsyncLocal
            CorrelationId.Value = null;
        }
    }

    [Fact]
    public void GetCurrent_ShouldReturn_NullCorrelationId_WhenNotSet()
    {
        // Ensure CorrelationId tidak di-set
        CorrelationId.Value = null;

        var accessor = new SystemActorAccessor();
        var actor = accessor.GetCurrent();

        actor.CorrelationId.Should().BeNull();
    }

    [Fact]
    public void GetCurrent_ShouldNeverReturnNull()
    {
        var accessor = new SystemActorAccessor();
        var actor = accessor.GetCurrent();

        actor.Should().NotBeNull();
    }

    [Fact]
    public void SystemActorAccessor_Implements_IActorAccessor()
    {
        typeof(SystemActorAccessor).Should().Implement<IActorAccessor>();
    }

    [Fact]
    public void AuditActor_System_ShouldBeStatic_Sentinel()
    {
        AuditActor.System.Actor.Should().Be("SYSTEM");
        AuditActor.System.ActorId.Should().BeNull();
        AuditActor.System.Role.Should().BeNull();
    }
}
