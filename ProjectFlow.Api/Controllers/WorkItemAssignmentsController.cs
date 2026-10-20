using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.API.Contracts.Assignments;
using ProjectFlow.Application.Features.Assignments.Commands.AssignWorkItem;
using ProjectFlow.Application.Features.Assignments.Commands.RemoveWorkItemAssignment;
using ProjectFlow.Application.Features.Assignments.Queries.GetWorkItemAssignments;

namespace ProjectFlow.API.Controllers;

[Authorize]
[ApiController]
[Route("api/work-items/{workItemId:guid}/assignments")]
public sealed class WorkItemAssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkItemAssignmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/work-items/{workItemId}/assignments
    [HttpGet]
    public async Task<IActionResult> GetAssignments(
        Guid workItemId,
        CancellationToken cancellationToken)
    {
        var assignments = await _mediator.Send(
            new GetWorkItemAssignmentsQuery(workItemId),
            cancellationToken);

        return Ok(assignments);
    }

    // POST: api/work-items/{workItemId}/assignments
    [HttpPost]
    public async Task<IActionResult> AssignUser(
        Guid workItemId,
        [FromBody] AssignWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        var assignmentId = await _mediator.Send(
            new AssignWorkItemCommand(workItemId, request.UserId),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetAssignments),
            new { workItemId },
            new { id = assignmentId });
    }

    // DELETE: api/work-items/{workItemId}/assignments/{assignmentId}
    [HttpDelete("{assignmentId:guid}")]
    public async Task<IActionResult> RemoveAssignment(
        Guid workItemId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var removed = await _mediator.Send(
            new RemoveWorkItemAssignmentCommand(assignmentId),
            cancellationToken);

        if (!removed)
            return NotFound();

        return NoContent();
    }
}