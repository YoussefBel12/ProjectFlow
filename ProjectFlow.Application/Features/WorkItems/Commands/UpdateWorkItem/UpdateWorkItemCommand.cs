
using MediatR;

namespace ProjectFlow.Application.Features.WorkItems.Commands.UpdateWorkItem;

public record UpdateWorkItemCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDateUtc) : IRequest<bool>;
