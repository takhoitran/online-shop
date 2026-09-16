using MediatR;
using OnlineShop.BuildingBlocks.Application;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed record DeactivateVoucherCommand(Guid VoucherId) : IRequest<Result>;
