
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Common.Interfaces;

public interface IProjectMemberRepository
{
    Task AddAsync(
        ProjectMember member,
        CancellationToken cancellationToken);

    Task<ProjectMember?> GetByProjectAndUserAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<ProjectMember?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProjectMember>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken);

    Task<int> CountManagersAsync(
        Guid projectId,
        CancellationToken cancellationToken);

    void Remove(ProjectMember member);



    Task<bool> IsMemberAsync(
    Guid projectId,
    Guid userId,
    CancellationToken cancellationToken);

}

