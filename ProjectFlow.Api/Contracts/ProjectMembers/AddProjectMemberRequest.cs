
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Api.Contracts.ProjectMembers;

public sealed record AddProjectMemberRequest(
    Guid UserId,
    ProjectRole Role = ProjectRole.Member);
