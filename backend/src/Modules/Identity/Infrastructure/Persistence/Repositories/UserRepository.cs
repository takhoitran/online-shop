using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default) =>
        _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AnyAsync(u => u.Username == username, cancellationToken);

    public void Add(User user) => _dbContext.Users.Add(user);
}
