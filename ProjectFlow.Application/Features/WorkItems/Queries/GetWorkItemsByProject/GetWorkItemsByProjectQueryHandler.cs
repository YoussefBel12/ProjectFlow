
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemsByProject;

public class GetWorkItemsByProjectQueryHandler
    : IRequestHandler<GetWorkItemsByProjectQuery, IReadOnlyList<WorkItem>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetWorkItemsByProjectQueryHandler(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<WorkItem>> Handle(
        GetWorkItemsByProjectQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue || userId.Value == Guid.Empty)
            throw new UnauthorizedAccessException(
                "Authentication is required.");

        var project = await _projectRepository.GetByIdAsync(
            request.ProjectId,
            cancellationToken);

        if (project is null)
            throw new KeyNotFoundException(
                $"Project with ID '{request.ProjectId}' was not found.");

        var isMember = await _memberRepository.IsMemberAsync(
            project.Id,
            userId.Value,
            cancellationToken);

        if (!isMember)
            throw new UnauthorizedAccessException(
                "You are not a member of this project.");

        return await _projectRepository.GetByProjectIdAsync(
            request.ProjectId,
            cancellationToken);
    }
}
