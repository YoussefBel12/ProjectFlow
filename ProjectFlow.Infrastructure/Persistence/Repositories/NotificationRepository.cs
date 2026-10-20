using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly ProjectFlowDbContext _context;

    public NotificationRepository(ProjectFlowDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken)
    {
        await _context.Set<Notification>()
            .AddAsync(notification, cancellationToken);
    }

    public async Task<IReadOnlyList<Notification>> GetByRecipientAsync(
        Guid recipientUserId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<Notification>()
            .AsNoTracking()
            .Where(n => n.RecipientUserId == recipientUserId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<Notification>()
            .FirstOrDefaultAsync(
                n => n.Id == notificationId,
                cancellationToken);
    }
}