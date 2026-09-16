using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Identity.Application.ChangePassword;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result>;
