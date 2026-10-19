using MediatR;

namespace ProjectFlow.Application.Features.WorkItems.Commands.DeleteWorkItem;

public record DeleteWorkItemCommand(Guid Id) : IRequest<bool>;