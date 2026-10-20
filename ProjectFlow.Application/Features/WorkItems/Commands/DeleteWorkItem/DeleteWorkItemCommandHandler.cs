
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.DeleteWorkItem;

public class DeleteWorkItemCommandHandler
    : IRequestHandler<DeleteWorkItemCommand, bool>
{
    private readonly IProjectRepository _repository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWorkItemCommandHandler(
        IProjectRepository repository,
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        DeleteWorkItemCommand request,
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

        if (membership.Role != ProjectRole.Manager)
            throw new UnauthorizedAccessException(
                "Only project managers can delete work items.");

        _repository.RemoveWorkItem(workItem);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
