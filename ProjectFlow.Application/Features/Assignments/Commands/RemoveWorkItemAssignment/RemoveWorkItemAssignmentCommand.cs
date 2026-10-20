using MediatR;

namespace ProjectFlow.Application.Features.Assignments.Commands.RemoveWorkItemAssignment;

public sealed record RemoveWorkItemAssignmentCommand(
    Guid AssignmentId
) : IRequest<bool>;