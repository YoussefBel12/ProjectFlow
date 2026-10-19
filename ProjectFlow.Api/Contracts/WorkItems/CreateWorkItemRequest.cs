namespace ProjectFlow.Api.Contracts.WorkItems
{

    

    public record CreateWorkItemRequest(
        Guid ProjectId,
        string Title,
        string? Description,
        DateTime? DueDateUtc);

}
