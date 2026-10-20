using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.Features.Notifications.Commands.MarkNotificationAsRead;
using ProjectFlow.Application.Features.Notifications.Queries.GetMyNotifications;

namespace ProjectFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        CancellationToken cancellationToken)
    {
        var notifications = await _mediator.Send(
            new GetMyNotificationsQuery(),
            cancellationToken);

        return Ok(notifications);
    }

    // PATCH: api/notifications/{notificationId}/read
    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var success = await _mediator.Send(
            new MarkNotificationAsReadCommand(notificationId),
            cancellationToken);

        if (!success)
            return NotFound();

        return NoContent();
    }
}