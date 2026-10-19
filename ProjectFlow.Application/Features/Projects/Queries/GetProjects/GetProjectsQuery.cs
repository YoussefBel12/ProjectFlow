using MediatR;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery : IRequest<IReadOnlyList<Project>>;