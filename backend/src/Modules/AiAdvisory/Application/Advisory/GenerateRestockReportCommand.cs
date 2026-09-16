using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.AiAdvisory.Application.Dtos;

namespace OnlineShop.Modules.AiAdvisory.Application.Advisory;

/// <summary>Seller/Admin-only — enforced by [Authorize] at the Api layer, not here (Application
/// stays authorization-agnostic, same convention as every other module).</summary>
public sealed record GenerateRestockReportCommand(Guid? SellerUserId) : IRequest<Result<RestockReportDto>>;
