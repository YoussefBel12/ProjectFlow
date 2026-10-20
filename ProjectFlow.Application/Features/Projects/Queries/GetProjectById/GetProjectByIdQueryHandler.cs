
using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler
    : IRequestHandler<GetProjectByIdQuery, Project?>
{
    private readonly IProjectRepository _repository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectByIdQueryHandler(
        IProjectRepository repository,
        IProjectMemberRepository memberRepository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _memberRepository = memberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Project?> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (project is null)
            return null;

        var userId = _currentUserService.UserId;

        if (!userId.HasValue)
            return null;

        var membership = await _memberRepository.GetByProjectAndUserAsync(
            request.Id,
            userId.Value,
            cancellationToken);

        return membership is null ? null : project;
    }
}
