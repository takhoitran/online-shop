using FluentValidation;

namespace OnlineShop.Modules.Identity.Application.TelegramLink;

public sealed class ConfirmTelegramLinkCommandValidator : AbstractValidator<ConfirmTelegramLinkCommand>
{
    public ConfirmTelegramLinkCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().Length(6);
        RuleFor(x => x.TelegramId).NotEmpty();
    }
}
