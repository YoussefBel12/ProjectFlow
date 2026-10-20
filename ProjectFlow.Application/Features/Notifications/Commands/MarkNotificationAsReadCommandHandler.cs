using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var notification = await _notificationRepository.GetByIdAsync(
            request.NotificationId,
            cancellationToken);

        // Don't allow users to read or modify another user's notification.
        if (notification is null || notification.RecipientUserId != userId)
            return false;

        notification.MarkAsRead();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}