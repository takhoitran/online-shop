using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Infrastructure.Seeding;

/// <summary>
/// Bootstraps the very first Admin account so there is someone who can log in and create Seller
/// accounts (public self-registration is Buyer-only — see RegisterUserCommand). Runs the real
/// IPasswordHasher instead of a hardcoded hash literal in a migration, so it stays correct even
/// if the hashing algorithm changes later.
/// </summary>
public static class IdentitySeeder
{
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminPassword = "Admin@123";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var roleRepository = services.GetRequiredService<IRoleRepository>();
        var userRepository = services.GetRequiredService<IUserRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var unitOfWork = services.GetRequiredService<IIdentityUnitOfWork>();

        var adminUsername = Username.Create(DefaultAdminUsername);
        if (await userRepository.ExistsByUsernameAsync(adminUsername, cancellationToken))
            return;

        var adminRole = await roleRepository.GetByNameAsync(RoleNames.Admin, cancellationToken)
            ?? throw new InvalidOperationException("Role 'Admin' has not been seeded — check RoleConfiguration/migration.");

        var admin = User.Register(
            adminUsername,
            passwordHasher.Hash(DefaultAdminPassword),
            "System Administrator",
            adminRole.Id);

        userRepository.Add(admin);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
