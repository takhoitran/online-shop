namespace OnlineShop.Modules.Catalog.Application.Dtos;

public sealed record ProductReviewDto(
    Guid Id,
    Guid ProductId,
    Guid BuyerId,
    string BuyerName,
    int Rating,
    string? Comment,
    DateTime CreatedAtUtc);
