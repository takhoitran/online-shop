using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _dbContext;

    public RoleRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _dbContext.Roles.SingleOrDefaultAsync(r => r.Name == name, cancellationToken);

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Roles.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
}
