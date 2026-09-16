using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed record CreateVoucherCommand(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    int? MaxUses,
    DateTime? ExpiresAtUtc) : IRequest<Result<Guid>>;
