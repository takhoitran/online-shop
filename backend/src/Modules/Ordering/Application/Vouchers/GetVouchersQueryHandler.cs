using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed class GetVouchersQueryHandler : IRequestHandler<GetVouchersQuery, List<VoucherDto>>
{
    private readonly IVoucherRepository _voucherRepository;

    public GetVouchersQueryHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<List<VoucherDto>> Handle(GetVouchersQuery request, CancellationToken cancellationToken)
    {
        var vouchers = await _voucherRepository.GetAllAsync(cancellationToken);
        return vouchers.Select(v => new VoucherDto(
            v.Id, v.Code, v.DiscountType.ToString(), v.DiscountValue, v.MaxUses, v.UsedCount, v.ExpiresAtUtc, v.IsActive, v.CreatedAtUtc)).ToList();
    }
}
