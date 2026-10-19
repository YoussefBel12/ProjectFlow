using MediatR;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.Projects.Queries.GetProjectById;

public record GetProjectByIdQuery(Guid Id) : IRequest<Project?>;