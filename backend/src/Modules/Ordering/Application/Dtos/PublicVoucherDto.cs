namespace OnlineShop.Modules.Ordering.Application.Dtos;

/// <summary>Buyer-facing voucher summary — no usage stats or admin fields.</summary>
public sealed record PublicVoucherDto(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    DateTime? ExpiresAtUtc);
