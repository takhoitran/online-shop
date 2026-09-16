namespace OnlineShop.Modules.Catalog.Application.Dtos;

public sealed record ProductListItemDto(
    Guid Id,
    string Name,
    string? NameEn,
    string? NameVi,
    decimal Price,
    string Currency,
    string? ImageUrl,
    int StockQuantity,
    Guid CategoryId,
    string CategoryName,
    string? CategoryNameEn,
    string? CategoryNameVi,
    double? AverageRating,
    int ReviewCount);
