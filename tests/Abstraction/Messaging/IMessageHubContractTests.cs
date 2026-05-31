using FluentAssertions;
using NDB.Platform.Abstraction.Messaging;
using NSubstitute;
using Xunit;

namespace NDB.Platform.Tests.Abstraction.Messaging;

/// <summary>
/// Contract tests untuk IMessageHub.
/// Verifikasi interface dapat dimock dan signature sesuai.
/// </summary>
public sealed class IMessageHubContractTests
{
    [Fact]
    public async Task PushMessageAsync_CallsWithCorrectArguments()
    {
        var hub = Substitute.For<IMessageHub>();
        var threadId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var senderId = Guid.NewGuid();

        await hub.PushMessageAsync(threadId, messageId, senderId, "Hello world");

        await hub.Received(1).PushMessageAsync(
            threadId, messageId, senderId, "Hello world", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PushMessageAsync_CanBeCalledWithCancellationToken()
    {
        var hub = Substitute.For<IMessageHub>();
        using var cts = new CancellationTokenSource();

        await hub.PushMessageAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "msg", cts.Token);

        await hub.Received(1).PushMessageAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(),
            "msg", cts.Token);
    }

    [Fact]
    public async Task PushMessageAsync_CalledMultipleTimes_CountsCorrectly()
    {
        var hub = Substitute.For<IMessageHub>();

        await hub.PushMessageAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "first");
        await hub.PushMessageAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "second");

        await hub.Received(2).PushMessageAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(),
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
