
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemById;

public class GetWorkItemByIdQueryHandler
    : IRequestHandler<GetWorkItemByIdQuery, WorkItem?>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetWorkItemByIdQueryHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<WorkItem?> Handle(
        GetWorkItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue || userId.Value == Guid.Empty)
            return null;

        var workItem = await _projectRepository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);

        if (workItem is null)
            return null;

        var isMember = await _memberRepository.IsMemberAsync(
            workItem.ProjectId,
            userId.Value,
            cancellationToken);

        return isMember ? workItem : null;
    }
}
