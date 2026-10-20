
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.ProjectActivities;
using ProjectFlow.Application.Common.Interfaces;

namespace ProjectFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/activities")]
public sealed class ProjectActivitiesController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _projectMemberRepository;
    private readonly IProjectActivityRepository _activityRepository;
    private readonly ICurrentUserService _currentUserService;

    public ProjectActivitiesController(
        IProjectRepository projectRepository,
        IProjectMemberRepository projectMemberRepository,
        IProjectActivityRepository activityRepository,
        ICurrentUserService currentUserService)
    {
        _projectRepository = projectRepository;
        _projectMemberRepository = projectMemberRepository;
        _activityRepository = activityRepository;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectActivityResponse>>> GetByProject(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            projectId,
            cancellationToken);

        if (project is null)
            return NotFound("Project not found.");

        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
            return Unauthorized();

        var isMember = await _projectMemberRepository.IsMemberAsync(
            projectId,
            userId.Value,
            cancellationToken);

        if (!isMember)
            return Forbid();

        var activities = await _activityRepository.GetByProjectIdAsync(
            projectId,
            cancellationToken);

        var response = activities
            .Select(activity => new ProjectActivityResponse(
                activity.Id,
                activity.ProjectId,
                activity.ActorUserId,
                activity.WorkItemId,
                activity.Type,
                activity.Details,
                activity.CreatedAtUtc))
            .ToList();

        return Ok(response);
    }
}
