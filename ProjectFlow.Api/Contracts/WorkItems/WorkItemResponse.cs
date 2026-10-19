namespace ProjectFlow.Api.Contracts.WorkItems
{

  

    public record WorkItemResponse(
        Guid Id,
        Guid ProjectId,
        string Title,
        string? Description,
        int Status,
        int Priority,
        DateTime? DueDateUtc,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);

}
