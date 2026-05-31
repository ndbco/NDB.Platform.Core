using FluentAssertions;
using NDB.Platform.Abstraction.Notification;
using NSubstitute;
using Xunit;

namespace NDB.Platform.Tests.Abstraction.Notification;

/// <summary>
/// Contract tests untuk INotificationDispatcher dan INotificationHub.
/// Verifikasi interface dapat dimock dan signature sesuai.
/// </summary>
public sealed class NotificationInterfaceContractTests
{
    // ── INotificationDispatcher ──────────────────────────────────────────────

    [Fact]
    public async Task INotificationDispatcher_DispatchAsync_ReturnNotificationId()
    {
        var dispatcher = Substitute.For<INotificationDispatcher>();
        var expected = Guid.NewGuid();
        dispatcher.DispatchAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await dispatcher.DispatchAsync(
            Guid.NewGuid(), "TASK", "Task assigned", "You have a new task");

        result.Should().Be(expected);
    }

    [Fact]
    public async Task INotificationDispatcher_DispatchAsync_CanReturnNull()
    {
        var dispatcher = Substitute.For<INotificationDispatcher>();
        dispatcher.DispatchAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await dispatcher.DispatchAsync(
            Guid.NewGuid(), "SYSTEM", "Notification blocked", null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task INotificationDispatcher_DefaultPriority_IsNormal()
    {
        var dispatcher = Substitute.For<INotificationDispatcher>();
        dispatcher.DispatchAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            "NORMAL", Arg.Any<CancellationToken>())
            .Returns(Guid.NewGuid());

        // Call without priority — default should be "NORMAL"
        await dispatcher.DispatchAsync(Guid.NewGuid(), "TASK", "Test", null);

        await dispatcher.Received(1).DispatchAsync(
            Arg.Any<Guid>(), "TASK", "Test", null,
            null, null, "NORMAL", Arg.Any<CancellationToken>());
    }

    // ── INotificationHub ─────────────────────────────────────────────────────

    [Fact]
    public async Task INotificationHub_PushNotificationAsync_CallsWithCorrectArgs()
    {
        var hub = Substitute.For<INotificationHub>();
        var recipientId = Guid.NewGuid();
        var notifId = Guid.NewGuid();

        await hub.PushNotificationAsync(recipientId, notifId, "Title", "Body");

        await hub.Received(1).PushNotificationAsync(
            recipientId, notifId, "Title", "Body", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task INotificationHub_PushNotificationAsync_NullBodyAllowed()
    {
        var hub = Substitute.For<INotificationHub>();

        await hub.PushNotificationAsync(Guid.NewGuid(), Guid.NewGuid(), "Title", null);

        await hub.Received(1).PushNotificationAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), "Title", null, Arg.Any<CancellationToken>());
    }
}
