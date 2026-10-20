
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Common.Interfaces;

public interface IProjectAssignmentRepository
{
    Task AddAsync(
        ProjectAssignment assignment,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProjectAssignment>> GetByWorkItemIdAsync(
        Guid workItemId,
        CancellationToken cancellationToken);

    Task<ProjectAssignment?> GetByIdAsync(
        Guid assignmentId,
        CancellationToken cancellationToken);

    Task<bool> ExistsAsync(
        Guid workItemId,
        Guid userId,
        CancellationToken cancellationToken);

    void Remove(ProjectAssignment assignment);
}
