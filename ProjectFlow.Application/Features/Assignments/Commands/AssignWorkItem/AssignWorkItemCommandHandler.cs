
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.Assignments.Commands.AssignWorkItem;

public sealed class AssignWorkItemCommandHandler
    : IRequestHandler<AssignWorkItemCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly IProjectActivityRepository _activityRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignWorkItemCommandHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IProjectAssignmentRepository assignmentRepository,
        IProjectActivityRepository activityRepository,
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _assignmentRepository = assignmentRepository;
        _activityRepository = activityRepository;
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        AssignWorkItemCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId == null || currentUserId == Guid.Empty)
            throw new UnauthorizedAccessException("Authentication required.");

        if (request.UserId == Guid.Empty)
            throw new ArgumentException("User ID is required.");

        var workItem = await _projectRepository
            .GetWorkItemByIdAsync(request.WorkItemId, cancellationToken);

        if (workItem == null)
            throw new KeyNotFoundException("Work item not found.");

        var manager = await _projectMemberRepository
            .GetByProjectAndUserAsync(
                workItem.ProjectId,
                currentUserId.Value,
                cancellationToken);

        if (manager == null || manager.Role != ProjectRole.Manager)
            throw new UnauthorizedAccessException(
                "Only project managers can assign work items.");

        var targetMember = await _projectMemberRepository
            .GetByProjectAndUserAsync(
                workItem.ProjectId,
                request.UserId,
                cancellationToken);

        if (targetMember == null)
            throw new ArgumentException(
                "The assigned user must be a member of this project.");

        var alreadyAssigned = await _assignmentRepository.ExistsAsync(
            request.WorkItemId,
            request.UserId,
            cancellationToken);

        if (alreadyAssigned)
            throw new InvalidOperationException(
                "This user is already assigned to the work item.");

        var assignment = new ProjectAssignment(
            request.WorkItemId,
            request.UserId);

        var activity = new ProjectActivity(
            workItem.ProjectId,
            currentUserId.Value,
            ActivityType.WorkItemAssigned,
            $"Assigned work item '{workItem.Title}' to user {request.UserId}",
            request.WorkItemId);

        var notification = new Notification(
            request.UserId,
            "New work item assignment",
            $"You have been assigned to '{workItem.Title}'.");

        await _assignmentRepository.AddAsync(
            assignment, cancellationToken);

        await _activityRepository.AddAsync(
            activity, cancellationToken);

        await _notificationRepository.AddAsync(
            notification, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
