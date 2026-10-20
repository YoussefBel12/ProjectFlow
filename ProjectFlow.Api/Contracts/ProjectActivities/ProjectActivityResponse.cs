
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Api.Contracts.ProjectActivities;

public sealed record ProjectActivityResponse(
    Guid Id,
    Guid ProjectId,
    Guid ActorUserId,
    Guid? WorkItemId,
    ActivityType Type,
    string Details,
    DateTime CreatedAtUtc);
