using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Infrastructure.Persistence;

namespace ProjectFlow.Infrastructure.BackgroundJobs;

public sealed class NotificationCleanupJob : INotificationCleanupJob
{
    private readonly ITenantStore _tenantStore;
    private readonly ILogger<NotificationCleanupJob> _logger;

    public NotificationCleanupJob(
        ITenantStore tenantStore,
        ILogger<NotificationCleanupJob> logger)
    {
        _tenantStore = tenantStore;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var tenants = await _tenantStore.GetActiveTenantsAsync(
            cancellationToken);

        var cutoffUtc = DateTime.UtcNow.AddDays(-90);

        foreach (var tenant in tenants)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var options =
                    new DbContextOptionsBuilder<ProjectFlowDbContext>()
                        .UseSqlServer(tenant.ConnectionString)
                        .Options;

                await using var context =
                    new ProjectFlowDbContext(options);

                var deletedCount = await context.Notifications
                    .Where(notification =>
                        notification.ReadAtUtc.HasValue &&
                        notification.ReadAtUtc.Value < cutoffUtc)
                    .ExecuteDeleteAsync(cancellationToken);

                _logger.LogInformation(
                    "Deleted {DeletedCount} old read notifications for tenant {TenantSlug}.",
                    deletedCount,
                    tenant.Slug);
            }
            catch (Exception exception)
                when (exception is not OperationCanceledException)
            {
                _logger.LogError(
                    exception,
                    "Notification cleanup failed for tenant {TenantSlug}.",
                    tenant.Slug);
            }
        }
    }
}