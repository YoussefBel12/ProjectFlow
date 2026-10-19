
using MediatR;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemsByProject;

public record GetWorkItemsByProjectQuery(Guid ProjectId)
    : IRequest<IReadOnlyList<WorkItem>>;
