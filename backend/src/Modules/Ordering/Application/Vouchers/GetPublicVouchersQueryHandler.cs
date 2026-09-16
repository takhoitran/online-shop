using MediatR;
using OnlineShop.Modules.Ordering.Application.Dtos;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed class GetPublicVouchersQueryHandler : IRequestHandler<GetPublicVouchersQuery, List<PublicVoucherDto>>
{
    private readonly IVoucherRepository _voucherRepository;

    public GetPublicVouchersQueryHandler(IVoucherRepository voucherRepository)
    {
        _voucherRepository = voucherRepository;
    }

    public async Task<List<PublicVoucherDto>> Handle(GetPublicVouchersQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var vouchers = await _voucherRepository.GetAllAsync(cancellationToken);
        return vouchers
            .Where(v => v.CanBeUsed(now))
            .OrderBy(v => v.ExpiresAtUtc ?? DateTime.MaxValue)
            .Select(v => new PublicVoucherDto(v.Code, v.DiscountType.ToString(), v.DiscountValue, v.ExpiresAtUtc))
            .ToList();
    }
}
