using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.Modules.Identity.Domain;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(Username username, CancellationToken cancellationToken = default);
    void Add(User user);
}
