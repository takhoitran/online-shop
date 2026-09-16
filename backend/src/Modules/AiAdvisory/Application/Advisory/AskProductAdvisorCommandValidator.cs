using FluentValidation;

namespace OnlineShop.Modules.AiAdvisory.Application.Advisory;

public sealed class AskProductAdvisorCommandValidator : AbstractValidator<AskProductAdvisorCommand>
{
    public AskProductAdvisorCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Channel).NotEmpty();
    }
}
