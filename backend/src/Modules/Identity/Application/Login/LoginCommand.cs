using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<Result<AuthResultDto>>;
