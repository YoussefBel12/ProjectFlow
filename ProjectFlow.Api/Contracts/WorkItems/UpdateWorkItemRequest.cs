
namespace ProjectFlow.Api.Contracts.WorkItems;

public record UpdateWorkItemRequest(
    string Title,
    string? Description,
    DateTime? DueDateUtc);
