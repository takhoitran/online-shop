using OnlineShop.Modules.AiAdvisory.Domain;

namespace OnlineShop.Modules.AiAdvisory.Application.Abstractions;

public interface IAiInteractionLogRepository
{
    void Add(AiInteractionLog log);
}
