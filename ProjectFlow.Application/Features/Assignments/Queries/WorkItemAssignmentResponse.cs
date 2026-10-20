namespace ProjectFlow.Application.Features.Assignments.Queries.GetWorkItemAssignments;

public sealed record WorkItemAssignmentResponse(
    Guid Id,
    Guid WorkItemId,
    Guid UserId,
    DateTime CreatedAtUtc
);