using FluentAssertions;
using NDB.Platform.Http.Resilience;
using Xunit;

namespace NDB.Platform.Tests.Http.Resilience;

public sealed class SessionExpiredEventArgsTests
{
    [Fact]
    public void SessionExpiredEventArgs_ShouldStoreReason()
    {
        var args = new SessionExpiredEventArgs("Refresh token expired");
        args.Reason.Should().Be("Refresh token expired");
    }

    [Fact]
    public void SessionExpiredEventArgs_NullReason_ShouldBeNull()
    {
        var args = new SessionExpiredEventArgs(null);
        args.Reason.Should().BeNull();
    }

    [Fact]
    public void SessionExpiredEventArgs_ShouldBeEventArgs()
    {
        var args = new SessionExpiredEventArgs("reason");
        args.Should().BeAssignableTo<EventArgs>();
    }
}
