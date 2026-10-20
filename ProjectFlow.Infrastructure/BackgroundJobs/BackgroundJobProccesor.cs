using Microsoft.Extensions.Logging;

namespace ProjectFlow.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobProcessor
{
    private readonly ILogger<BackgroundJobProcessor> _logger;

    public BackgroundJobProcessor(
        ILogger<BackgroundJobProcessor> logger)
    {
        _logger = logger;
    }

    public void ProcessLogMessage(string message)
    {
        _logger.LogInformation(
            "Hangfire background job executed. Message: {Message}",
            message);
    }
}