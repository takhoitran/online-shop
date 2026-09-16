namespace OnlineShop.Modules.Identity.Domain;

public interface ITelegramLinkCodeRepository
{
    Task<TelegramLinkCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    void Add(TelegramLinkCode linkCode);
}
