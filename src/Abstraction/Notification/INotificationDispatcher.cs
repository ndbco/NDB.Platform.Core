namespace NDB.Platform.Abstraction.Notification;

/// <summary>
/// Abstraction for a multi-channel notification dispatcher.
/// Fan-out to IN_APP, EMAIL, or PUSH based on user preferences and DND windows.
/// The implementation is provided by the consuming project (with DB and channel provider access).
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>
    /// Dispatches a notification to all active channels for the given user.
    /// </summary>
    /// <param name="recipientId">User ID of the recipient.</param>
    /// <param name="category">Notification category (e.g. "TASK", "REMINDER", "SYSTEM").</param>
    /// <param name="title">Notification title.</param>
    /// <param name="body">Notification body (optional).</param>
    /// <param name="actionUrl">Optional action URL (deep link or web URL).</param>
    /// <param name="notifModule">Optional originating module (e.g. "workflow", "dms").</param>
    /// <param name="priority">Priority: "NORMAL", "HIGH", or "URGENT". Default: "NORMAL".</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The notification ID created in-app, or null if all channels are blocked.</returns>
    Task<Guid?> DispatchAsync(
        Guid recipientId,
        string category,
        string title,
        string? body,
        string? actionUrl = null,
        string? notifModule = null,
        string priority = "NORMAL",
        CancellationToken ct = default);
}
