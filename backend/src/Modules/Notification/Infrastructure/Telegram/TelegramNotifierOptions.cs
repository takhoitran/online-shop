namespace OnlineShop.Modules.Notification.Infrastructure.Telegram;

/// <summary>Reads the same "TelegramBot:BotToken" config key the TelegramBot project's polling
/// client uses — a deliberate duplicate of that tiny options shape rather than a project
/// reference to TelegramBot (a Presentation-layer channel adapter, not something a module's
/// Infrastructure should depend on). Two independent TelegramBotClient instances sharing one bot
/// token is fine: only long-polling (GetUpdates) needs exclusivity, not SendMessage.</summary>
public sealed class TelegramNotifierOptions
{
    public const string SectionName = "TelegramBot";

    public string BotToken { get; set; } = string.Empty;
}
