namespace ProjectFlow.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc
);