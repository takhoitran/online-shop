using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Identity.Domain;

/// <summary>
/// Well-known role names — the ubiquitous language shared with the Use Case actors.
/// Admin: full system access, manages staff (Seller) accounts — see Identity.Application.Users.
/// Seller: operational staff — manages catalog/inventory/orders, cannot manage other accounts.
/// Buyer: customer — the only role open to public self-registration.
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Seller = "Seller";
    public const string Buyer = "Buyer";
}

public sealed class Role : Entity<Guid>
{
    public string Name { get; private set; } = default!;

    private Role() { }

    private Role(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Role Create(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        return new Role(Guid.NewGuid(), name.Trim());
    }
}
