using OnlineShop.Modules.Identity.Application.Dtos;

namespace OnlineShop.Modules.Identity.Application.Abstractions;

/// <summary>Read-side port for the Admin "Users" screen — same reasoning as
/// Catalog's IProductCatalogQueries: a plain projection, no need to rehydrate User aggregates.</summary>
public interface IUserDirectoryQueries
{
    Task<List<UserSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Resolves a Telegram chat to the OnlineShop account linked to it — how the
    /// Telegram Channel Adapter (Phase 7) knows who it's talking to, without its own auth.</summary>
    Task<UserSummaryDto?> GetByTelegramIdAsync(string telegramId, CancellationToken cancellationToken = default);

    /// <summary>The reverse of GetByTelegramIdAsync — given a user, the raw chat id to push a
    /// Telegram message to (null if never linked). Used by Notification (Phase 9) to decide
    /// whether a given order/stock event can be pushed to Telegram, not just logged in-app.</summary>
    Task<string?> GetTelegramIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
