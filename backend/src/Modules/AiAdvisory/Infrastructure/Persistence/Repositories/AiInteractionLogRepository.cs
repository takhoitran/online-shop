using OnlineShop.Modules.AiAdvisory.Application.Abstractions;
using OnlineShop.Modules.AiAdvisory.Domain;

namespace OnlineShop.Modules.AiAdvisory.Infrastructure.Persistence.Repositories;

internal sealed class AiInteractionLogRepository : IAiInteractionLogRepository
{
    private readonly AiAdvisoryDbContext _dbContext;

    public AiInteractionLogRepository(AiAdvisoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(AiInteractionLog log) => _dbContext.AiInteractionLogs.Add(log);
}
