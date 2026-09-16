using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Ordering.Application.Orders;

namespace OnlineShop.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
    private bool IsStaff => User.IsInRole("Seller") || User.IsInRole("Admin");

    [Authorize(Roles = "Buyer")]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request, CancellationToken cancellationToken)
    {
        var command = new CheckoutCommand(
            CurrentUserId,
            request.PaymentMethod,
            request.RecipientName,
            request.PhoneNumber,
            request.AddressLine,
            request.City,
            request.VoucherCode,
            request.ProductIds);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    /// <summary>Buyer sees only their own orders; Seller/Admin sees everyone's (e.g. the Pending
    /// queue waiting for approval) — matches the original "View new orders" seller flow.</summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = IsStaff ? null : (Guid?)CurrentUserId;
        var query = new GetOrdersQuery(userId, status, page, pageSize);
        return Ok(await _sender.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (order is null)
            return NotFound();
        if (!IsStaff && order.UserId != CurrentUserId)
            return Forbid();

        return Ok(order);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancelOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(id, CurrentUserId, IsStaff, request.Reason);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new ApproveOrderCommand(id, CurrentUserId), cancellationToken)).ToActionResult();

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new MarkOrderPaidCommand(id), cancellationToken)).ToActionResult();

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new ShipOrderCommand(id), cancellationToken)).ToActionResult();

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new CompleteOrderCommand(id), cancellationToken)).ToActionResult();

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id, ReturnOrderRequest request, CancellationToken cancellationToken) =>
        (await _sender.Send(new ReturnOrderCommand(id, CurrentUserId, request.Reason), cancellationToken)).ToActionResult();
}

public sealed record CheckoutRequest(
    string PaymentMethod,
    string RecipientName,
    string PhoneNumber,
    string AddressLine,
    string City,
    string? VoucherCode = null,
    IReadOnlyList<Guid>? ProductIds = null);
public sealed record CancelOrderRequest(string? Reason);
public sealed record ReturnOrderRequest(string? Reason);
