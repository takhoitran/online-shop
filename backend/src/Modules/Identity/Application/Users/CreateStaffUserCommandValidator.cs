using FluentValidation;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Application.Users;

public sealed class CreateStaffUserCommandValidator : AbstractValidator<CreateStaffUserCommand>
{
    public CreateStaffUserCommandValidator()
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

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(role => role is RoleNames.Seller or RoleNames.Admin)
            .WithMessage($"Role must be '{RoleNames.Seller}' or '{RoleNames.Admin}'.");
    }
}
