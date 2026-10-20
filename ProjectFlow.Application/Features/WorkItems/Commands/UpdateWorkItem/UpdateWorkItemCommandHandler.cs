
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.UpdateWorkItem;

public class UpdateWorkItemCommandHandler
    : IRequestHandler<UpdateWorkItemCommand, bool>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWorkItemCommandHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateWorkItemCommand request,
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

        if (membership.Role != ProjectRole.Manager)
            throw new UnauthorizedAccessException(
                "Only project managers can edit work item details.");

        workItem.UpdateDetails(
            request.Title,
            request.Description,
            request.DueDateUtc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
