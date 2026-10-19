
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Persistence.Repositories;

public class WorkItemCommentRepository : IWorkItemCommentRepository
{
    private readonly ProjectFlowDbContext _context;

    public WorkItemCommentRepository(ProjectFlowDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        WorkItemComment comment,
        CancellationToken cancellationToken)
    {
        await _context.WorkItemComments.AddAsync(
            comment, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkItemComment>> GetByWorkItemIdAsync(
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        return await _context.WorkItemComments
            .AsNoTracking()
            .Where(comment => comment.WorkItemId == workItemId)
            .OrderBy(comment => comment.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkItemComment?> GetByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken)
    {
        return await _context.WorkItemComments
            .FirstOrDefaultAsync(
                comment => comment.Id == commentId,
                cancellationToken);
    }

    public void Remove(WorkItemComment comment)
    {
        _context.WorkItemComments.Remove(comment);
    }
}
