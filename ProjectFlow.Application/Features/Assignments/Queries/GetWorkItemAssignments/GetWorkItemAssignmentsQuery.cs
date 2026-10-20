using MediatR;

namespace ProjectFlow.Application.Features.Assignments.Queries.GetWorkItemAssignments;

public sealed record GetWorkItemAssignmentsQuery(
    Guid WorkItemId
) : IRequest<IReadOnlyList<WorkItemAssignmentResponse>>;