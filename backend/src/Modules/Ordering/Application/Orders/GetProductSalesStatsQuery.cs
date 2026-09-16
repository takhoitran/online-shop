using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Orders;

/// <summary>The "sales" half of AI Advisory's restock report (Phase 8) — AiAdvisory calls
/// this via ISender instead of ever touching Ordering's DbContext, per the Context Map.</summary>
public sealed record GetProductSalesStatsQuery(int TopN = 30) : IRequest<IReadOnlyList<ProductSalesStatsDto>>;
