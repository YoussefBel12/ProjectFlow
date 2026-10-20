using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid recipientUserId,
        CancellationToken cancellationToken);

    Task<Notification?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken);
}