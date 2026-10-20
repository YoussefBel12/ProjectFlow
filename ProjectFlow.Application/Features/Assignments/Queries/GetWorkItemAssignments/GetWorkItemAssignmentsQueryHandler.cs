using MediatR;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Application.Features.Assignments.Queries.GetWorkItemAssignments;

public sealed class GetWorkItemAssignmentsQueryHandler
    : IRequestHandler<
        GetWorkItemAssignmentsQuery,
        IReadOnlyList<WorkItemAssignmentResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetWorkItemAssignmentsQueryHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IProjectAssignmentRepository assignmentRepository,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _assignmentRepository = assignmentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<WorkItemAssignmentResponse>> Handle(
        GetWorkItemAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId == null || currentUserId == Guid.Empty)
            throw new UnauthorizedAccessException("Authentication required.");

        var workItem = await _projectRepository.GetWorkItemByIdAsync(
            request.WorkItemId,
            cancellationToken);

        if (workItem == null)
            throw new KeyNotFoundException("Work item not found.");

        var membership = await _projectMemberRepository
            .GetByProjectAndUserAsync(
                workItem.ProjectId,
                currentUserId.Value,
                cancellationToken);

        if (membership == null)
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");

        var assignments = await _assignmentRepository.GetByWorkItemIdAsync(
            request.WorkItemId,
            cancellationToken);

        return assignments
            .Select(a => new WorkItemAssignmentResponse(
                a.Id,
                a.WorkItemId,
                a.UserId,
                a.CreatedAtUtc))
            .ToList();
    }
}