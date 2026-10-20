
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Api.Contracts.WorkItems;
using ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemPriority;
using ProjectFlow.Application.Features.WorkItems.Commands.ChangeWorkItemStatus;
using ProjectFlow.Application.Features.WorkItems.Commands.CreateWorkItem;
using ProjectFlow.Application.Features.WorkItems.Commands.DeleteWorkItem;
using ProjectFlow.Application.Features.WorkItems.Commands.UpdateWorkItem;
using ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemById;
using ProjectFlow.Application.Features.WorkItems.Queries.GetWorkItemsByProject;
using ProjectFlow.Domain.Enums;
namespace ProjectFlow.Api.Controllers
{




    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WorkItemsController : ControllerBase
    {
        private readonly ISender _sender;

        public WorkItemsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateWorkItemRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateWorkItemCommand(
                request.ProjectId,
                request.Title,
                request.Description,
                request.DueDateUtc);

            var workItemId = await _sender.Send(
                command,
                cancellationToken);

            return Created(
                $"/api/WorkItems/{workItemId}",
                new { id = workItemId });
        }

      
        [HttpGet("project/{projectId:guid}")]
        public async Task<IActionResult> GetByProject(
    Guid projectId,
    CancellationToken cancellationToken)
        {
            var query = new GetWorkItemsByProjectQuery(projectId);

            var workItems = await _sender.Send(query, cancellationToken);

            var response = workItems.Select(workItem =>
                new WorkItemResponse(
                    workItem.Id,
                    workItem.ProjectId,
                    workItem.Title,
                    workItem.Description,
                    (int)workItem.Status,
                    (int)workItem.Priority,
                    workItem.DueDateUtc,
                    workItem.CreatedAtUtc,
                    workItem.UpdatedAtUtc));

            return Ok(response);
        }



        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
        {
            var workItem = await _sender.Send(
                new GetWorkItemByIdQuery(id),
                cancellationToken);

            if (workItem is null)
                return NotFound();

            var response = new WorkItemResponse(
                workItem.Id,
                workItem.ProjectId,
                workItem.Title,
                workItem.Description,
                (int)workItem.Status,
                (int)workItem.Priority,
                workItem.DueDateUtc,
                workItem.CreatedAtUtc,
                workItem.UpdatedAtUtc);

            return Ok(response);
        }



        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateWorkItemRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateWorkItemCommand(
                id,
                request.Title,
                request.Description,
                request.DueDateUtc);

            var updated = await _sender.Send(
                command,
                cancellationToken);

            if (!updated)
                return NotFound();

            return NoContent();
        }



        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ChangeStatus(
            Guid id,
            ChangeWorkItemStatusRequest request,
            CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(WorkItemStatus), request.Status))
                return BadRequest("Invalid work item status.");

            var command = new ChangeWorkItemStatusCommand(
                id,
                (WorkItemStatus)request.Status);

            var updated = await _sender.Send(
                command,
                cancellationToken);

            if (!updated)
                return NotFound();

            return NoContent();
        }


        [HttpPatch("{id:guid}/priority")]
        public async Task<IActionResult> ChangePriority(
    Guid id,
    ChangeWorkItemPriorityRequest request,
    CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(typeof(WorkItemPriority), request.Priority))
                return BadRequest("Invalid work item priority.");

            var command = new ChangeWorkItemPriorityCommand(
                id,
                (WorkItemPriority)request.Priority);

            var updated = await _sender.Send(command, cancellationToken);

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
                new DeleteWorkItemCommand(id),
                cancellationToken);

            if (!deleted)
                return NotFound();

            return NoContent();
        }






    }
}