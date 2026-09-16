namespace OnlineShop.Modules.Ordering.Application.Dtos;

/// <summary>Aggregated demand signal for one product, across confirmed orders only (Pending
/// carries no real signal yet, Cancelled/Returned reversed it) — the input AI Advisory's
/// restock report (Phase 8) reads instead of touching Ordering's DbContext directly.</summary>
public sealed record ProductSalesStatsDto(Guid ProductId, string ProductName, int TotalQuantitySold, decimal TotalRevenue, string Currency);
