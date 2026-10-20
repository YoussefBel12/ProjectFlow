using MediatR;

namespace ProjectFlow.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId)
    : IRequest<bool>;