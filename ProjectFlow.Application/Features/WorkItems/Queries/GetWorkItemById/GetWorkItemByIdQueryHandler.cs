using MediatR;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemById;

public class GetWorkItemByIdQueryHandler
    : IRequestHandler<GetWorkItemByIdQuery, WorkItem?>
{
    private readonly IProjectRepository _projectRepository;

    public GetWorkItemByIdQueryHandler(
        IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<WorkItem?> Handle(
        GetWorkItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _projectRepository.GetWorkItemByIdAsync(
            request.Id,
            cancellationToken);
    }
}