
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Repositories;

public sealed class ProjectActivityRepository : IProjectActivityRepository
{
    private readonly ProjectFlowDbContext _context;

    public ProjectActivityRepository(ProjectFlowDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        ProjectActivity activity,
        CancellationToken cancellationToken)
    {
        await _context.Set<ProjectActivity>()
            .AddAsync(activity, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectActivity>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<ProjectActivity>()
            .AsNoTracking()
            .Where(activity => activity.ProjectId == projectId)
            .OrderByDescending(activity => activity.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
