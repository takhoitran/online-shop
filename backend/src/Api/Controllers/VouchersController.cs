using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Ordering.Application.Vouchers;

namespace OnlineShop.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/vouchers")]
public sealed class VouchersController : ControllerBase
{
    private readonly ISender _sender;

    public VouchersController(ISender sender)
    {
        _sender = sender;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Active, usable promo codes for the storefront deals page (no auth required).</summary>
    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetPublicVouchersQuery(), cancellationToken));

    [Authorize(Roles = "Seller,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetVouchersQuery(), cancellationToken));

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateVoucherRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateVoucherCommand(request.Code, request.DiscountType, request.DiscountValue, request.MaxUses, request.ExpiresAtUtc);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateVoucherRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateVoucherCommand(id, request.DiscountType, request.DiscountValue, request.MaxUses, request.ExpiresAtUtc);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken) =>
        (await _sender.Send(new DeactivateVoucherCommand(id), cancellationToken)).ToActionResult();

    [Authorize(Roles = "Buyer")]
    [HttpGet("preview")]
    public async Task<IActionResult> Preview(
        [FromQuery] string code, [FromQuery] Guid[]? productIds, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new PreviewVoucherQuery(CurrentUserId, code, productIds), cancellationToken));
}

public sealed record CreateVoucherRequest(string Code, string DiscountType, decimal DiscountValue, int? MaxUses, DateTime? ExpiresAtUtc);
public sealed record UpdateVoucherRequest(string DiscountType, decimal DiscountValue, int? MaxUses, DateTime? ExpiresAtUtc);
