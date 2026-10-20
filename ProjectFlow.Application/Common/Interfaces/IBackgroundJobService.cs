namespace ProjectFlow.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    void EnqueueLogMessage(string message);
}