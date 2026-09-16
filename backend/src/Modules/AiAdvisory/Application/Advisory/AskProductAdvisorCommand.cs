using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.AiAdvisory.Application.Advisory;

/// <summary>Channel is "Web" or "Telegram" — same command either way, matching how every other
/// cross-channel action in this system (cart, checkout, ...) is one Application command called
/// from two Presentation layers. UserId is null for a not-logged-in Guest asking on the Web.</summary>
public sealed record AskProductAdvisorCommand(string Message, Guid? UserId, string Channel) : IRequest<Result<string>>;
