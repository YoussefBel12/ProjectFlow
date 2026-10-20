
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Repositories;

public sealed class ProjectAssignmentRepository
    : IProjectAssignmentRepository
{
    private readonly ProjectFlowDbContext _context;

    public ProjectAssignmentRepository(ProjectFlowDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        ProjectAssignment assignment,
        CancellationToken cancellationToken)
    {
        await _context.Set<ProjectAssignment>()
            .AddAsync(assignment, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectAssignment>> GetByWorkItemIdAsync(
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<ProjectAssignment>()
            .AsNoTracking()
            .Where(a => a.WorkItemId == workItemId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectAssignment?> GetByIdAsync(
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<ProjectAssignment>()
            .FirstOrDefaultAsync(
                a => a.Id == assignmentId,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid workItemId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.Set<ProjectAssignment>()
            .AnyAsync(
                a => a.WorkItemId == workItemId && a.UserId == userId,
                cancellationToken);
    }

    public void Remove(ProjectAssignment assignment)
    {
        _context.Set<ProjectAssignment>().Remove(assignment);
    }
}
