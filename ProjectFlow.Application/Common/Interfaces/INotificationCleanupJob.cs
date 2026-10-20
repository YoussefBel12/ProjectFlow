namespace ProjectFlow.Application.Common.Interfaces;

public interface INotificationCleanupJob
{
    Task ExecuteAsync(
        CancellationToken cancellationToken = default);
}