using MediatR;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemById;

public record GetWorkItemByIdQuery(Guid Id) : IRequest<WorkItem?>;