using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Identity.Application.ChangePassword;
using OnlineShop.Modules.Identity.Application.Login;
using OnlineShop.Modules.Identity.Application.Register;
using OnlineShop.Modules.Identity.Application.TelegramLink;

namespace OnlineShop.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken) =>
        (await _sender.Send(command, cancellationToken)).ToActionResult();

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken) =>
        (await _sender.Send(command, cancellationToken)).ToActionResult();

    [Authorize]
    [HttpPost("telegram-link/generate")]
    public async Task<IActionResult> GenerateTelegramLinkCode(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var result = await _sender.Send(new GenerateTelegramLinkCodeCommand(userId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Called by the Telegram Channel Adapter (Phase 7), not by the Web frontend directly.</summary>
    [HttpPost("telegram-link/confirm")]
    public async Task<IActionResult> ConfirmTelegramLink(ConfirmTelegramLinkCommand command, CancellationToken cancellationToken) =>
        (await _sender.Send(command, cancellationToken)).ToActionResult();

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        return (await _sender.Send(command, cancellationToken)).ToActionResult();
    }
}

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
