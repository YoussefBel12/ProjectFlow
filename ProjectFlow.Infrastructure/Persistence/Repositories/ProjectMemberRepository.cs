
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Infrastructure.Persistence.Repositories;

public class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly ProjectFlowDbContext _context;

    public ProjectMemberRepository(ProjectFlowDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        ProjectMember member,
        CancellationToken cancellationToken)
    {
        await _context.ProjectMembers.AddAsync(member, cancellationToken);
    }

    public async Task<ProjectMember?> GetByProjectAndUserAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.ProjectMembers.FirstOrDefaultAsync(
            member => member.ProjectId == projectId &&
                      member.UserId == userId,
            cancellationToken);
    }

    public async Task<ProjectMember?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.ProjectMembers.FirstOrDefaultAsync(
            member => member.Id == id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectMember>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return await _context.ProjectMembers
            .AsNoTracking()
            .Where(member => member.ProjectId == projectId)
            .OrderBy(member => member.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountManagersAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return await _context.ProjectMembers.CountAsync(
            member => member.ProjectId == projectId &&
                      member.Role == ProjectRole.Manager,
            cancellationToken);
    }

    public void Remove(ProjectMember member)
    {
        _context.ProjectMembers.Remove(member);
    }

    public async Task<bool> IsMemberAsync(
    Guid projectId,
    Guid userId,
    CancellationToken cancellationToken)
    {
        return await _context.ProjectMembers.AnyAsync(
            member => member.ProjectId == projectId
                && member.UserId == userId,
            cancellationToken);
    }



}
