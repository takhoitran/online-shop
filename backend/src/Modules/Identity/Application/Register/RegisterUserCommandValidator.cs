using FluentValidation;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Application.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(Username.MinLength, Username.MaxLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);
    }
}
