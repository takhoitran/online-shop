namespace OnlineShop.TelegramBot;

public sealed class TelegramBotOptions
{
    public const string SectionName = "TelegramBot";

    /// <summary>Empty/null disables the bot entirely — see TelegramBotHostedService.</summary>
    public string BotToken { get; set; } = string.Empty;
}
