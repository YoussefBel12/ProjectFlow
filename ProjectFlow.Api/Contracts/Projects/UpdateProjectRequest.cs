namespace ProjectFlow.Api.Contracts.Projects;

public record UpdateProjectRequest(
    string Name,
    string? Description,
    DateTime? DueDateUtc);