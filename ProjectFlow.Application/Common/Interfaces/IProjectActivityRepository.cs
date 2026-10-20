
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Common.Interfaces;

public interface IProjectActivityRepository
{
    Task AddAsync(
        ProjectActivity activity,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProjectActivity>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken);
}
