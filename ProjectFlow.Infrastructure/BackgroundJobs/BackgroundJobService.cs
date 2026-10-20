using Hangfire;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public BackgroundJobService(
        IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueLogMessage(string message)
    {
        _backgroundJobClient.Enqueue<BackgroundJobProcessor>(
            processor => processor.ProcessLogMessage(message));
    }
}