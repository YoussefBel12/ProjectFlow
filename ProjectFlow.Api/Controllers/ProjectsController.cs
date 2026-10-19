using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.Projects;
using ProjectFlow.Application.Features.Projects.Commands.CreateProject;

namespace ProjectFlow.Api.Controllers
{
   

    [ApiController]
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

            var projectId = await _sender.Send(command, cancellationToken);

            return Created($"/api/projects/{projectId}", new { id = projectId });
        }
    }
}
