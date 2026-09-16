using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Modules.Notification.Application.Abstractions;
using Telegram.Bot;

namespace OnlineShop.Modules.Notification.Infrastructure.Telegram;

internal sealed class TelegramNotifier : ITelegramNotifier
{
    private readonly TelegramNotifierOptions _options;
    private readonly ILogger<TelegramNotifier> _logger;
    private ITelegramBotClient? _client;

    public TelegramNotifier(IOptions<TelegramNotifierOptions> options, ILogger<TelegramNotifier> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string telegramId, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken))
        {
            _logger.LogDebug("TelegramBot:BotToken not configured — skipping push notification.");
            return;
        }

        if (!long.TryParse(telegramId, out var chatId))
        {
            _logger.LogWarning("TelegramId {TelegramId} is not a valid chat id — skipping push notification.", telegramId);
            return;
        }

        _client ??= new TelegramBotClient(_options.BotToken);
        await _client.SendMessage(chatId, message, cancellationToken: cancellationToken);
    }
}
