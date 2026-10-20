
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemPriority;

public class ChangeWorkItemPriorityCommandHandler
    : IRequestHandler<ChangeWorkItemPriorityCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IProjectActivityRepository _activityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeWorkItemPriorityCommandHandler(
        IProjectRepository repository,
        IProjectMemberRepository memberRepository,
        IProjectActivityRepository activityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _activityRepository = activityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        ChangeWorkItemPriorityCommand request,
        CancellationToken cancellationToken)
    {
        var workItem = await _repository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);

        if (workItem is null)
            return false;

        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
            throw new UnauthorizedAccessException(
                "Authentication is required.");

        var membership = await _memberRepository.GetByProjectAndUserAsync(
            workItem.ProjectId,
            userId.Value,
            cancellationToken);

        if (membership is null)
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");

        workItem.ChangePriority(request.Priority);

        var activity = new ProjectActivity(
            workItem.ProjectId,
            userId.Value,
            ActivityType.WorkItemPriorityChanged,
            $"Changed work item '{workItem.Title}' priority to {request.Priority}",
            workItem.Id);

        await _activityRepository.AddAsync(
            activity,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
