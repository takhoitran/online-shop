namespace OnlineShop.Modules.Notification.Application.Abstractions;

/// <summary>Outbound-only push to a Telegram chat, independent of the TelegramBot project's
/// long-polling receiver (see Phase 7) — sending a message needs no exclusive connection, so this
/// module owns its own lightweight sender rather than depending on the Presentation-layer bot
/// adapter. Implemented in Infrastructure directly against the Telegram Bot API.</summary>
public interface ITelegramNotifier
{
    Task SendAsync(string telegramId, string message, CancellationToken cancellationToken = default);
}
