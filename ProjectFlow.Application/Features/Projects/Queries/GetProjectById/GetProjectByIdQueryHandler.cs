using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.Projects.Queries.GetProjectById;

public class GetProjectByIdQueryHandler
    : IRequestHandler<GetProjectByIdQuery, Project?>
{
    private readonly IProjectRepository _repository;

    public GetProjectByIdQueryHandler(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task<Project?> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);
    }
}