
using MediatR;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemStatus;

public record ChangeWorkItemStatusCommand(
    Guid Id,
    WorkItemStatus Status) : IRequest<bool>;
