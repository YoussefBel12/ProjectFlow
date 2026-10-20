
namespace ProjectFlow.Api.Contracts.ProjectMembers;

public sealed record AvailableProjectUserResponse(
    Guid UserId,
    string? DisplayName,
    string? UserName,
    string? Email);
