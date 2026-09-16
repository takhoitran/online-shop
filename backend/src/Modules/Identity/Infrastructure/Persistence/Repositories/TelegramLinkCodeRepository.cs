using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Identity.Domain;

namespace OnlineShop.Modules.Identity.Infrastructure.Persistence.Repositories;

internal sealed class TelegramLinkCodeRepository : ITelegramLinkCodeRepository
{
    private readonly IdentityDbContext _dbContext;

    public TelegramLinkCodeRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TelegramLinkCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        _dbContext.TelegramLinkCodes.SingleOrDefaultAsync(c => c.Code == code, cancellationToken);

    public void Add(TelegramLinkCode linkCode) => _dbContext.TelegramLinkCodes.Add(linkCode);
}
