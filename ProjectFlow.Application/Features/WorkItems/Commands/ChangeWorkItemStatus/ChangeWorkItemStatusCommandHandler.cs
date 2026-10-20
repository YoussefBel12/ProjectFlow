
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemStatus;

public class ChangeWorkItemStatusCommandHandler
    : IRequestHandler<ChangeWorkItemStatusCommand, bool>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IProjectActivityRepository _projectActivityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeWorkItemStatusCommandHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        IProjectActivityRepository projectActivityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _projectActivityRepository = projectActivityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        ChangeWorkItemStatusCommand request,
        CancellationToken cancellationToken)
    {
        var workItem = await _projectRepository.GetWorkItemByIdAsync(
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

        workItem.ChangeStatus(request.Status);

        var activity = new ProjectActivity(
            workItem.ProjectId,
            userId.Value,
            ActivityType.WorkItemStatusChanged,
            $"Changed work item '{workItem.Title}' status to {request.Status}",
            workItem.Id);

        await _projectActivityRepository.AddAsync(
            activity,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
