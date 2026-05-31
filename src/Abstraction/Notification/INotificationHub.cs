namespace NDB.Platform.Abstraction.Notification;

/// <summary>
/// Abstraction for pushing real-time notifications per user.
/// The implementation is provided by the consuming project (SignalR <c>IHubContext</c>, WebSocket, SSE, etc.).
/// </summary>
public interface INotificationHub
{
    /// <summary>
    /// Pushes a notification to a user group (identified by userId).
    /// </summary>
    /// <param name="recipientId">User ID of the recipient.</param>
    /// <param name="notificationId">ID of the newly created notification.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="body">Notification body (optional).</param>
    /// <param name="ct">Cancellation token.</param>
    Task PushNotificationAsync(
        Guid recipientId,
        Guid notificationId,
        string title,
        string? body,
        CancellationToken ct = default);
}
