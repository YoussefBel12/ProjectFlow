using MediatR;

namespace ProjectFlow.Application.Features.Projects.Commands.DeleteProject;

public record DeleteProjectCommand(Guid Id) : IRequest<bool>;