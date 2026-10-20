using MediatR;

namespace ProjectFlow.Application.Features.Assignments.Commands.AssignWorkItem;

public sealed record AssignWorkItemCommand(
    Guid WorkItemId,
    Guid UserId
) : IRequest<Guid>;