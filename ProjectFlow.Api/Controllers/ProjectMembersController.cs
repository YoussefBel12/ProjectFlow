
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.ProjectMembers;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;
using ProjectFlow.Infrastructure.Identity;

namespace ProjectFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/members")]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectMembersController(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(
        Guid projectId,
        AddProjectMemberRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            projectId, cancellationToken);

        if (project is null)
            return NotFound("Project not found.");

        var userId = _currentUserService.UserId;

        if (userId is null)
            return Unauthorized();

        var requester = await _memberRepository.GetByProjectAndUserAsync(
            projectId, userId.Value, cancellationToken);

        if (requester?.Role != ProjectRole.Manager)
            return Forbid();

        if (request.UserId == Guid.Empty)
            return BadRequest("A valid user ID is required.");

        var targetUser = await _userManager.FindByIdAsync(
            request.UserId.ToString());

        if (targetUser is null)
            return NotFound("The user does not exist.");

        var existingMember =
            await _memberRepository.GetByProjectAndUserAsync(
                projectId, request.UserId, cancellationToken);

        if (existingMember is not null)
            return Conflict("This user is already a project member.");

        var member = new ProjectMember(
            projectId,
            request.UserId,
            ProjectRole.Member);

        await _memberRepository.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = ToResponse(member);

        return CreatedAtAction(
            nameof(GetMembers),
            new { projectId },
            response);
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            projectId, cancellationToken);

        if (project is null)
            return NotFound("Project not found.");

        var userId = _currentUserService.UserId;

        if (userId is null)
            return Unauthorized();

        var isMember = await _memberRepository.IsMemberAsync(
            projectId, userId.Value, cancellationToken);

        if (!isMember)
            return Forbid();

        var members = await _memberRepository.GetByProjectIdAsync(
            projectId, cancellationToken);

        return Ok(members.Select(ToResponse));
    }

    [HttpPatch("{memberId:guid}/role")]
    public async Task<IActionResult> ChangeRole(
        Guid projectId,
        Guid memberId,
        ChangeProjectMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            projectId, cancellationToken);

        if (project is null)
            return NotFound("Project not found.");

        var userId = _currentUserService.UserId;

        if (userId is null)
            return Unauthorized();

        var requester = await _memberRepository.GetByProjectAndUserAsync(
            projectId, userId.Value, cancellationToken);

        if (requester?.Role != ProjectRole.Manager)
            return Forbid();

        if (!Enum.IsDefined(request.Role))
            return BadRequest("Invalid project role.");

        var member = await _memberRepository.GetByIdAsync(
            memberId, cancellationToken);

        if (member is null || member.ProjectId != projectId)
            return NotFound("Project member not found.");

        if (member.Role == request.Role)
            return NoContent();

        if (member.Role == ProjectRole.Manager &&
            request.Role != ProjectRole.Manager)
        {
            var managerCount = await _memberRepository.CountManagersAsync(
                projectId, cancellationToken);

            if (managerCount <= 1)
                return Conflict(
                    "The project must have at least one manager.");
        }

        member.ChangeRole(request.Role);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(
        Guid projectId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            projectId, cancellationToken);

        if (project is null)
            return NotFound("Project not found.");

        var userId = _currentUserService.UserId;

        if (userId is null)
            return Unauthorized();

        var requester = await _memberRepository.GetByProjectAndUserAsync(
            projectId, userId.Value, cancellationToken);

        if (requester?.Role != ProjectRole.Manager)
            return Forbid();

        var member = await _memberRepository.GetByIdAsync(
            memberId, cancellationToken);

        if (member is null || member.ProjectId != projectId)
            return NotFound("Project member not found.");

        if (member.Role == ProjectRole.Manager)
        {
            var managerCount = await _memberRepository.CountManagersAsync(
                projectId, cancellationToken);

            if (managerCount <= 1)
                return Conflict(
                    "The last project manager cannot be removed.");
        }

        _memberRepository.Remove(member);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ProjectMemberResponse ToResponse(
        ProjectMember member)
    {
        return new ProjectMemberResponse(
            member.Id,
            member.ProjectId,
            member.UserId,
            member.Role,
            member.CreatedAtUtc);
    }
}
