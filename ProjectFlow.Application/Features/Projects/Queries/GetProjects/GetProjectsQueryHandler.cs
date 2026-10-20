
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler
    : IRequestHandler<GetProjectsQuery, IReadOnlyList<Project>>
{
    private readonly IProjectRepository _repository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectsQueryHandler(
        IProjectRepository repository,
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<Project>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
            return Array.Empty<Project>();

        var projects = await _repository.GetAllAsync(cancellationToken);

        var memberships = await _memberRepository.GetByUserIdAsync(
            userId.Value,
            cancellationToken);

        var projectIds = memberships
            .Select(membership => membership.ProjectId)
            .ToHashSet();

        return projects
            .Where(project => projectIds.Contains(project.Id))
            .ToList();
    }
}
