
using MediatR;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemPriority;

public record ChangeWorkItemPriorityCommand(
    Guid Id,
    WorkItemPriority Priority) : IRequest<bool>;
