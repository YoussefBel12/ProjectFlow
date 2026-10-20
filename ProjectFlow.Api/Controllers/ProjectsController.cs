
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.Projects;
using ProjectFlow.Application.Features.Projects.Commands.CreateProject;
using ProjectFlow.Application.Features.Projects.Commands.DeleteProject;
using ProjectFlow.Application.Features.Projects.Commands.UpdateProject;
using ProjectFlow.Application.Features.Projects.Queries.GetProjectById;
using ProjectFlow.Application.Features.Projects.Queries.GetProjects;

namespace ProjectFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            request.Name,
            request.Description,
            request.DueDateUtc);

        var projectId = await _sender.Send(
            command,
            cancellationToken);

        return Created(
            $"/api/projects/{projectId}",
            new { id = projectId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _sender.Send(
            new UpdateProjectCommand(
                id,
                request.Name,
                request.Description,
                request.DueDateUtc),
            cancellationToken);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(
            new DeleteProjectCommand(id),
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var project = await _sender.Send(
            new GetProjectByIdQuery(id),
            cancellationToken);

        if (project is null)
            return NotFound();

        var response = new ProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.DueDateUtc,
            project.CreatedAtUtc,
            project.UpdatedAtUtc);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var projects = await _sender.Send(
            new GetProjectsQuery(),
            cancellationToken);

        var response = projects.Select(project => new ProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.DueDateUtc,
            project.CreatedAtUtc,
            project.UpdatedAtUtc));

        return Ok(response);
    }
}
