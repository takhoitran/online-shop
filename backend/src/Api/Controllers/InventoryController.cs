using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Inventory.Application.GetProductStock;
using OnlineShop.Modules.Inventory.Application.IssueStock;
using OnlineShop.Modules.Inventory.Application.ReceiveStock;

namespace OnlineShop.Api.Controllers;

[Authorize(Roles = "Seller,Admin")]
[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly ISender _sender;

    public InventoryController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetStock(Guid productId, CancellationToken cancellationToken)
    {
        var stock = await _sender.Send(new GetProductStockQuery(productId), cancellationToken);
        return stock is null ? NotFound() : Ok(stock);
    }

    /// <summary>Receive stock — adds to the quantity on hand, records one IN history row.</summary>
    [HttpPost("receive")]
    public async Task<IActionResult> Receive(ReceiveStockRequest request, CancellationToken cancellationToken)
    {
        var command = new ReceiveStockCommand(request.ProductId, request.Quantity, CurrentUserId, request.Note);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    /// <summary>Manual stock issue (write-off, stocktake) — not tied to any order.</summary>
    [HttpPost("issue")]
    public async Task<IActionResult> Issue(IssueStockRequest request, CancellationToken cancellationToken)
    {
        var command = new IssueStockCommand(request.ProductId, request.Quantity, CurrentUserId, request.Note);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }
}

public sealed record ReceiveStockRequest(Guid ProductId, int Quantity, string? Note);
public sealed record IssueStockRequest(Guid ProductId, int Quantity, string? Note);
