using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.Catalog.Application.Dtos;

namespace OnlineShop.Modules.Catalog.Application.Products;

/// <summary>
/// The shared search/filter entry point for Web, Telegram, and AI Advisory (Open Host Service —
/// see the Context Map) — everyone that needs "which products match X" calls this, never the
/// EF DbContext directly.
/// </summary>
public sealed record GetProductsQuery(
    string? Keyword,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    double? MinRating = null) : IRequest<PagedResult<ProductListItemDto>>;
