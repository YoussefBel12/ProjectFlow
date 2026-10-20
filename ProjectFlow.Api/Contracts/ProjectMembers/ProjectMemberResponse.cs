
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Api.Contracts.ProjectMembers;

public sealed record ProjectMemberResponse(
    Guid Id,
    Guid ProjectId,
    Guid UserId,
    ProjectRole Role,
    DateTime CreatedAtUtc,
    string? DisplayName,
    string? UserName,
    string? Email);
