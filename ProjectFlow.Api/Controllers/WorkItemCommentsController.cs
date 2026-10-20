
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.WorkItemComments;
using ProjectFlow.Application.Common.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/work-items/{workItemId:guid}/comments")]
public class WorkItemCommentsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IWorkItemCommentRepository _commentRepository;
    private readonly IProjectActivityRepository _activityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public WorkItemCommentsController(
        IProjectRepository projectRepository,
        IProjectMemberRepository memberRepository,
        IWorkItemCommentRepository commentRepository,
        IProjectActivityRepository activityRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _memberRepository = memberRepository;
        _commentRepository = commentRepository;
        _activityRepository = activityRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleWorkItemAsync(
            workItemId, cancellationToken);

        if (access.Error is not null)
            return access.Error;

        var comments = await _commentRepository.GetByWorkItemIdAsync(
            workItemId, cancellationToken);

        return Ok(comments.Select(ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment(
        Guid workItemId,
        CreateWorkItemCommentRequest request,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleWorkItemAsync(
            workItemId, cancellationToken);

        if (access.Error is not null)
            return access.Error;

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Comment content is required.");

        if (request.Content.Length > 2000)
            return BadRequest("Comment cannot exceed 2000 characters.");

        var workItem = await _projectRepository.GetWorkItemByIdAsync(
            workItemId, cancellationToken);

        if (workItem is null)
            return NotFound("Work item not found.");

        var comment = new WorkItemComment(
            workItemId,
            access.UserId!.Value,
            request.Content);

        var activity = new ProjectActivity(
            workItem.ProjectId,
            access.UserId.Value,
            ActivityType.CommentAdded,
            $"Added a comment to work item '{workItem.Title}'",
            workItem.Id);

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _activityRepository.AddAsync(activity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(comment));
    }

    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> EditComment(
        Guid workItemId,
        Guid commentId,
        CreateWorkItemCommentRequest request,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleWorkItemAsync(
            workItemId, cancellationToken);

        if (access.Error is not null)
            return access.Error;

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Comment content is required.");

        if (request.Content.Length > 2000)
            return BadRequest("Comment cannot exceed 2000 characters.");

        var comment = await _commentRepository.GetByIdAsync(
            commentId, cancellationToken);

        if (comment is null || comment.WorkItemId != workItemId)
            return NotFound("Comment not found.");

        var isAuthor = comment.AuthorUserId == access.UserId!.Value;
        var isManager = access.Role == ProjectRole.Manager;

        if (!isAuthor && !isManager)
            return Forbid();

        comment.Edit(request.Content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(
        Guid workItemId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var access = await GetAccessibleWorkItemAsync(
            workItemId, cancellationToken);

        if (access.Error is not null)
            return access.Error;

        var comment = await _commentRepository.GetByIdAsync(
            commentId, cancellationToken);

        if (comment is null || comment.WorkItemId != workItemId)
            return NotFound("Comment not found.");

        var isAuthor = comment.AuthorUserId == access.UserId!.Value;
        var isManager = access.Role == ProjectRole.Manager;

        if (!isAuthor && !isManager)
            return Forbid();

        _commentRepository.Remove(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<WorkItemAccessResult> GetAccessibleWorkItemAsync(
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (userId is null || userId == Guid.Empty)
            return new(null, null, Unauthorized());

        var workItem = await _projectRepository.GetWorkItemByIdAsync(
            workItemId, cancellationToken);

        if (workItem is null)
            return new(userId, null, NotFound("Work item not found."));

        var project = await _projectRepository.GetByIdAsync(
            workItem.ProjectId, cancellationToken);

        if (project is null)
            return new(userId, null, NotFound("Project not found."));

        var membership = await _memberRepository.GetByProjectAndUserAsync(
            project.Id, userId.Value, cancellationToken);

        if (membership is null)
            return new(userId, null, Forbid());

        return new(userId, membership.Role, null);
    }

    private static WorkItemCommentResponse ToResponse(
        WorkItemComment comment)
    {
        return new WorkItemCommentResponse(
            comment.Id,
            comment.WorkItemId,
            comment.AuthorUserId,
            comment.Content,
            comment.CreatedAtUtc,
            comment.UpdatedAtUtc);
    }

    private sealed record WorkItemAccessResult(
        Guid? UserId,
        ProjectRole? Role,
        IActionResult? Error);
}

public sealed record WorkItemCommentResponse(
    Guid Id,
    Guid WorkItemId,
    Guid AuthorUserId,
    string Content,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
