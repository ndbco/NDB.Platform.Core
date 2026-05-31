namespace NDB.Platform.Abstraction.Messaging;

/// <summary>
/// Abstraction for pushing real-time messages to thread participants.
/// The implementation is provided by the consuming project (SignalR <c>IHubContext</c>, WebSocket, SSE, etc.).
/// </summary>
public interface IMessageHub
{
    /// <summary>
    /// Pushes a new message to all members of a thread group.
    /// </summary>
    /// <param name="threadId">ID of the conversation thread.</param>
    /// <param name="messageId">ID of the newly sent message.</param>
    /// <param name="senderId">User ID of the sender.</param>
    /// <param name="body">Message body.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PushMessageAsync(
        Guid threadId,
        Guid messageId,
        Guid senderId,
        string body,
        CancellationToken ct = default);
}
