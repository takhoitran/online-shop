using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.AiAdvisory.Application.Advisory;

namespace OnlineShop.Api.Controllers;

/// <summary>Rate-limited (see Program.cs's "ai" policy) — every action here calls out to the
/// real Gemini API, unmetered on our side, so an abusive caller could burn a day's quota fast.</summary>
[EnableRateLimiting("ai")]
[ApiController]
[Route("api/ai")]
public sealed class AiAdvisoryController : ControllerBase
{
    private readonly ISender _sender;

    public AiAdvisoryController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Open to Guests as well as Buyers — matches the storefront allowing product
    /// browsing without login (see the plan's Use Case review, point 1). Only Cart/Checkout
    /// require an account. UserId is populated from the JWT when one is present, else logged as
    /// an anonymous interaction.</summary>
    [AllowAnonymous]
    [HttpPost("ask")]
    public async Task<IActionResult> Ask(AskProductAdvisorRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var userId = Guid.TryParse(userIdClaim, out var parsed) ? parsed : (Guid?)null;

        var command = new AskProductAdvisorCommand(request.Message, userId, "Web");
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpPost("restock-report")]
    public async Task<IActionResult> RestockReport(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        return (await _sender.Send(new GenerateRestockReportCommand(userId), cancellationToken)).ToActionResult();
    }
}

public sealed record AskProductAdvisorRequest(string Message);
