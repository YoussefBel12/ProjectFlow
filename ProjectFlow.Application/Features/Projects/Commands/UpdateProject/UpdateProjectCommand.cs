using MediatR;

namespace ProjectFlow.Application.Features.Projects.Commands.UpdateProject;

public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Description,
    DateTime? DueDateUtc) : IRequest<bool>;