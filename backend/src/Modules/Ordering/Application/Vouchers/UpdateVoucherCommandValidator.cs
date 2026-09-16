using FluentValidation;

namespace OnlineShop.Modules.Ordering.Application.Vouchers;

public sealed class UpdateVoucherCommandValidator : AbstractValidator<UpdateVoucherCommand>
{
    public UpdateVoucherCommandValidator()
    {
        RuleFor(x => x.VoucherId).NotEmpty();
        RuleFor(x => x.DiscountType).NotEmpty().Must(t => t is "Percentage" or "FixedAmount")
            .WithMessage("Discount type must be 'Percentage' or 'FixedAmount'.");
        RuleFor(x => x.DiscountValue).GreaterThan(0);
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses.HasValue);
    }
}
