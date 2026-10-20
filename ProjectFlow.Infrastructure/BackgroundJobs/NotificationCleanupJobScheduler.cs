using Hangfire;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Infrastructure.BackgroundJobs;

public static class NotificationCleanupJobScheduler
{
    public static void Register()
    {
        RecurringJob.AddOrUpdate<INotificationCleanupJob>(
            "notification-cleanup",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily);
    }
}
