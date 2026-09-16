using FluentValidation;

namespace OnlineShop.Modules.Inventory.Application.IssueStock;

public sealed class IssueStockCommandValidator : AbstractValidator<IssueStockCommand>
{
    public IssueStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).NotEmpty().MaximumLength(500)
            .WithMessage("Please state the reason for this manual stock issue (write-off, stocktake...).");
    }
}
