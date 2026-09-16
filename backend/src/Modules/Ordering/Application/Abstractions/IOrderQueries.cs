using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.Modules.Ordering.Application.Abstractions;

public sealed record OrderSearchFilter(Guid? UserId, string? Status, int Page = 1, int PageSize = 20);

public interface IOrderQueries
{
    Task<PagedResult<OrderSummaryDto>> SearchAsync(OrderSearchFilter filter, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>Per-product quantity sold + revenue across confirmed orders (Approved/Shipping/
    /// Completed), highest quantity first — feeds AI Advisory's restock report (Phase 8).</summary>
    Task<IReadOnlyList<ProductSalesStatsDto>> GetProductSalesStatsAsync(int topN, CancellationToken cancellationToken = default);
}
