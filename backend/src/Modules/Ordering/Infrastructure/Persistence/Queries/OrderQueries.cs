using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Application.Dtos;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Queries;

internal sealed class OrderQueries : IOrderQueries
{
    private readonly OrderingDbContext _dbContext;

    public OrderQueries(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<OrderSummaryDto>> SearchAsync(OrderSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Orders.AsNoTracking().AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(o => o.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<OrderStatus>(filter.Status, true, out var status))
            query = query.Where(o => o.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(o => o.OrderDateUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.UserId,
                o.Status.ToString(),
                o.PaymentMethod.ToString(),
                o.PaymentStatus.ToString(),
                o.TotalAmount.Amount,
                o.TotalAmount.Currency,
                o.Details.Count,
                o.OrderDateUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Details)
            .SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return null;

        var details = order.Details
            .Select(d => new OrderDetailLineDto(d.ProductId, d.ProductName, d.Quantity, d.UnitPrice.Amount, d.LineTotal.Amount))
            .ToList();

        return new OrderDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.PaymentMethod.ToString(),
            order.PaymentStatus.ToString(),
            order.Subtotal.Amount,
            order.DiscountAmount.Amount,
            order.VoucherCode,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            details,
            order.OrderDateUtc,
            order.UpdatedAtUtc,
            order.ShippingAddress.RecipientName,
            order.ShippingAddress.PhoneNumber,
            order.ShippingAddress.AddressLine,
            order.ShippingAddress.City);
    }

    public async Task<IReadOnlyList<ProductSalesStatsDto>> GetProductSalesStatsAsync(int topN, CancellationToken cancellationToken = default)
    {
        var confirmedStatuses = new[] { OrderStatus.Approved, OrderStatus.Shipping, OrderStatus.Completed };

        // Npgsql's EF Core provider can't translate Sum() over an owned-type property (UnitPrice.Amount)
        // inside a GroupBy — project the flat scalar rows first, then aggregate client-side (fine at this
        // shop's scale: one row per order line, not per order).
        var lines = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => confirmedStatuses.Contains(o.Status))
            .SelectMany(o => o.Details)
            .Select(d => new { d.ProductId, d.ProductName, d.Quantity, Amount = d.UnitPrice.Amount, d.UnitPrice.Currency })
            .ToListAsync(cancellationToken);

        return lines
            .GroupBy(l => new { l.ProductId, l.ProductName })
            .Select(g => new ProductSalesStatsDto(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(l => l.Quantity),
                g.Sum(l => l.Quantity * l.Amount),
                g.First().Currency))
            .OrderByDescending(s => s.TotalQuantitySold)
            .Take(Math.Clamp(topN, 1, 200))
            .ToList();
    }
}
