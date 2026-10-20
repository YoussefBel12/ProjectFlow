using MediatR;

namespace ProjectFlow.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQuery
    : IRequest<IReadOnlyList<NotificationResponse>>;