using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery,
        IReadOnlyList<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<NotificationResponse>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var notifications =
            await _notificationRepository.GetByRecipientAsync(
                userId,
                cancellationToken);

        return notifications
            .Select(n => new NotificationResponse(
                n.Id,
                n.Title,
                n.Message,
                n.ReadAtUtc.HasValue,
                n.CreatedAtUtc,
                n.ReadAtUtc))
            .ToList();
    }
}