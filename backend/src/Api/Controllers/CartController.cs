using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Ordering.Application.Carts;

namespace OnlineShop.Api.Controllers;

[Authorize(Roles = "Buyer")]
[ApiController]
[Route("api/cart")]
public sealed class CartController : ControllerBase
{
    private readonly ISender _sender;

    public CartController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetCartQuery(CurrentUserId), cancellationToken));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddCartItemCommand(CurrentUserId, request.ProductId, request.Quantity);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<IActionResult> UpdateItemQuantity(Guid productId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCartItemQuantityCommand(CurrentUserId, productId, request.Quantity);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken cancellationToken) =>
        (await _sender.Send(new RemoveCartItemCommand(CurrentUserId, productId), cancellationToken)).ToActionResult();
}

public sealed record AddCartItemRequest(Guid ProductId, int Quantity);
public sealed record UpdateCartItemRequest(int Quantity);
