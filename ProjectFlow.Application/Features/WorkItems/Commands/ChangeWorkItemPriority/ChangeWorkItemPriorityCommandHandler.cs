
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemPriority;

public class ChangeWorkItemPriorityCommandHandler
    : IRequestHandler<ChangeWorkItemPriorityCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IProjectActivityRepository _activityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeWorkItemPriorityCommandHandler(
        IProjectRepository repository,
        IProjectActivityRepository activityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _activityRepository = activityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        ChangeWorkItemPriorityCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated user is required to change work-item priority.");
        }

        var workItem = await _repository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);

        if (workItem is null)
            return false;

        workItem.ChangePriority(request.Priority);

        var activity = new ProjectActivity(
            workItem.ProjectId,
            userId.Value,
            ActivityType.WorkItemPriorityChanged,
            $"Changed work item '{workItem.Title}' priority to {request.Priority}",
            workItem.Id);

        await _activityRepository.AddAsync(activity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
