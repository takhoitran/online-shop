using FluentValidation;

namespace OnlineShop.Modules.Ordering.Application.Orders;

public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .Must(m => m is "Cod" or "BankTransfer")
            .WithMessage("Payment method must be 'Cod' or 'BankTransfer'.");

        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.AddressLine).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.VoucherCode).MaximumLength(50);
    }
}
