using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence.Queries;

internal sealed class UserDirectoryQueries : IUserDirectoryQueries
{
    private readonly IdentityDbContext _dbContext;

    public UserDirectoryQueries(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Select the whole User entity (not just `.Username.Value`) — EF Core materializes
        // value-converted properties fine on a full entity, but can't translate unwrapping a
        // converted property's member (.Value) into SQL inside a projection. Map to the DTO
        // in-memory instead, after the (small, admin-only) result set comes back.
        var rows = await (
            from user in _dbContext.Users.AsNoTracking()
            join role in _dbContext.Roles.AsNoTracking() on user.RoleId equals role.Id
            orderby user.CreatedAtUtc descending
            select new { user, roleName = role.Name })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new UserSummaryDto(
                r.user.Id,
                r.user.Username.Value,
                r.user.FullName,
                r.roleName,
                r.user.TelegramId != null,
                r.user.CreatedAtUtc))
            .ToList();
    }

    public async Task<UserSummaryDto?> GetByTelegramIdAsync(string telegramId, CancellationToken cancellationToken = default)
    {
        var row = await (
            from user in _dbContext.Users.AsNoTracking()
            join role in _dbContext.Roles.AsNoTracking() on user.RoleId equals role.Id
            where user.TelegramId == telegramId
            select new { user, roleName = role.Name })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new UserSummaryDto(row.user.Id, row.user.Username.Value, row.user.FullName, row.roleName, true, row.user.CreatedAtUtc);
    }

    public async Task<string?> GetTelegramIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.TelegramId)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
