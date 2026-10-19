namespace ProjectFlow.Api.Contracts.Projects
{
    

    public record CreateProjectRequest(
        string Name,
        string? Description,
        DateTime? DueDateUtc);
}
