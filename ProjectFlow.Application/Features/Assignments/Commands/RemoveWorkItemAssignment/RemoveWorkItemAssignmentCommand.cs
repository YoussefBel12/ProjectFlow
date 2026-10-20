using MediatR;

namespace ProjectFlow.Application.Features.Assignments.Commands.RemoveWorkItemAssignment;

public sealed record RemoveWorkItemAssignmentCommand(
    Guid WorkItemId,
    Guid AssignmentId
) : IRequest<bool>;