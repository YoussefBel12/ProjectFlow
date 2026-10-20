using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.Assignments.Commands.RemoveWorkItemAssignment;

public sealed class RemoveWorkItemAssignmentCommandHandler
    : IRequestHandler<RemoveWorkItemAssignmentCommand, bool>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly IProjectActivityRepository _activityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveWorkItemAssignmentCommandHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IProjectAssignmentRepository assignmentRepository,
        IProjectActivityRepository activityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _assignmentRepository = assignmentRepository;
        _activityRepository = activityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        RemoveWorkItemAssignmentCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId == null || currentUserId == Guid.Empty)
            throw new UnauthorizedAccessException("Authentication required.");

        var assignment = await _assignmentRepository.GetByIdAsync(
            request.AssignmentId,
            cancellationToken);

        if (assignment == null)
            return false;

        var workItem = await _projectRepository.GetWorkItemByIdAsync(
            assignment.WorkItemId,
            cancellationToken);

        if (workItem == null)
            return false;

        var manager = await _projectMemberRepository
            .GetByProjectAndUserAsync(
                workItem.ProjectId,
                currentUserId.Value,
                cancellationToken);

        if (manager == null || manager.Role != ProjectRole.Manager)
            throw new UnauthorizedAccessException(
                "Only project managers can remove assignments.");

        var activity = new ProjectActivity(
            workItem.ProjectId,
            currentUserId.Value,
            ActivityType.WorkItemAssigned,
            $"Removed user {assignment.UserId} from work item '{workItem.Title}'",
            assignment.WorkItemId);

        _assignmentRepository.Remove(assignment);

        await _activityRepository.AddAsync(
            activity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}