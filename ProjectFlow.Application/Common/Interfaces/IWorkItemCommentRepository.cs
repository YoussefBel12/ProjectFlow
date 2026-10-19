
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Common.Interfaces;

public interface IWorkItemCommentRepository
{
    Task AddAsync(
        WorkItemComment comment,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WorkItemComment>> GetByWorkItemIdAsync(
        Guid workItemId,
        CancellationToken cancellationToken);

    Task<WorkItemComment?> GetByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken);

    void Remove(WorkItemComment comment);
}
