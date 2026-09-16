namespace OnlineShop.Modules.Catalog.Application.Dtos;

public sealed record ProductDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string? NameEn,
    string? NameVi,
    string? DescriptionEn,
    string? DescriptionVi,
    decimal Price,
    string Currency,
    string? ImageUrl,
    int StockQuantity,
    Guid CategoryId,
    string CategoryName,
    string? CategoryNameEn,
    string? CategoryNameVi,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    double? AverageRating,
    int ReviewCount,
    IReadOnlyList<ProductImageDto> Images);
