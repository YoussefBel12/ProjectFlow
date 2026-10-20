using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dev/background-jobs")]
public sealed class BackgroundJobsController : ControllerBase
{
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IWebHostEnvironment _environment;

    public BackgroundJobsController(
        IBackgroundJobService backgroundJobService,
        IWebHostEnvironment environment)
    {
        _backgroundJobService = backgroundJobService;
        _environment = environment;
    }

    [HttpPost("log")]
    public IActionResult EnqueueLogMessage(
        [FromQuery] string message = "Hello from ProjectFlow!")
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest("Message cannot be empty.");
        }

        _backgroundJobService.EnqueueLogMessage(message);

        return Accepted(new
        {
            Message = "Background job queued successfully."
        });
    }
}