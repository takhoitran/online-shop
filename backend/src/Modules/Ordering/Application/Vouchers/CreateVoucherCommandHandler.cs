using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Ordering.Application.Abstractions;
using OnlineShop.Modules.Ordering.Domain;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, Result<Guid>>
{
    private readonly IVoucherRepository _voucherRepository;
    private readonly IOrderingUnitOfWork _unitOfWork;

    public CreateVoucherCommandHandler(IVoucherRepository voucherRepository, IOrderingUnitOfWork unitOfWork)
    {
        _voucherRepository = voucherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
    {
        var existing = await _voucherRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(new Error("Voucher.CodeAlreadyExists", "A voucher with this code already exists."));

        try
        {
            var voucher = Voucher.Create(
                request.Code,
                Enum.Parse<DiscountType>(request.DiscountType, ignoreCase: true),
                request.DiscountValue,
                request.MaxUses,
                request.ExpiresAtUtc);

            _voucherRepository.Add(voucher);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(voucher.Id);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(new Error("Voucher.Invalid", ex.Message));
        }
    }
}
