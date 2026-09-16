using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Ordering.Application.Wishlists;

namespace OnlineShop.Api.Controllers;

[Authorize(Roles = "Buyer")]
[ApiController]
[Route("api/wishlist")]
public sealed class WishlistController : ControllerBase
{
    private readonly ISender _sender;

    public WishlistController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetWishlistQuery(CurrentUserId), cancellationToken));

    [HttpPost("items/{productId:guid}")]
    public async Task<IActionResult> AddItem(Guid productId, CancellationToken cancellationToken) =>
        (await _sender.Send(new AddToWishlistCommand(CurrentUserId, productId), cancellationToken)).ToActionResult();

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken cancellationToken) =>
        (await _sender.Send(new RemoveFromWishlistCommand(CurrentUserId, productId), cancellationToken)).ToActionResult();
}
