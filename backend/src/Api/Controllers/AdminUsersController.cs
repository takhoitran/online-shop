using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Api.Extensions;
using OnlineShop.Modules.Identity.Application.Users;

namespace OnlineShop.Api.Controllers;

/// <summary>Admin-only user management — "User/Role management". Seller/Admin accounts are
/// provisioned here, never through public self-registration (see AuthController.Register).</summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminUsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetUsersQuery(), cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateStaffUserCommand command, CancellationToken cancellationToken) =>
        (await _sender.Send(command, cancellationToken)).ToActionResult();
}
