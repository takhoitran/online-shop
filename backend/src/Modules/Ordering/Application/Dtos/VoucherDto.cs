namespace OnlineShop.Modules.Ordering.Application.Dtos;

public sealed record VoucherDto(
    Guid Id,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    int? MaxUses,
    int UsedCount,
    DateTime? ExpiresAtUtc,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record VoucherPreviewDto(bool IsValid, string? ErrorMessage, decimal DiscountAmount, decimal NewTotal, string Currency);
