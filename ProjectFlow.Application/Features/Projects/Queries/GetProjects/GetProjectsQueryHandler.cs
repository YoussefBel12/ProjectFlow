using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler
    : IRequestHandler<GetProjectsQuery, IReadOnlyList<Project>>
{
    private readonly IProjectRepository _repository;

    public GetProjectsQueryHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Project>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}