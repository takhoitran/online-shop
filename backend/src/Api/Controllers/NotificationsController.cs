using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Notification.Application.Notifications;

namespace OnlineShop.Api.Controllers;

/// <summary>Every authenticated role has an in-app inbox — Buyers get order-status updates,
/// Seller/Admin get low-stock alerts (see NotifyOnOrderLifecycleEventsHandler / NotifyOnLowStockHandler).</summary>
[Authorize]
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await _sender.Send(new GetNotificationsQuery(CurrentUserId, page, pageSize), cancellationToken));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetUnreadNotificationCountQuery(CurrentUserId), cancellationToken));

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new MarkNotificationAsReadCommand(CurrentUserId, id), cancellationToken)).ToActionResult();
}
