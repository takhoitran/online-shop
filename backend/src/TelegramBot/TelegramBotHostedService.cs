using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace OnlineShop.TelegramBot;

/// <summary>
/// Runs inside the same host process as the Web Api (registered alongside it in Program.cs) —
/// a Modular Monolith is one process with multiple entry points, so this reuses the exact same
/// DI container, MediatR pipeline, and DbContexts rather than standing up a second app.
/// Uses long polling (GetUpdates), not a webhook: no public HTTPS URL is needed, which matters
/// for local development — switching to a webhook for production is a config-only change
/// (call SetWebhookAsync instead of StartReceiving), not a rewrite of TelegramUpdateDispatcher.
/// </summary>
public sealed class TelegramBotHostedService : BackgroundService
{
    private readonly TelegramBotOptions _options;
    private readonly TelegramUpdateDispatcher _dispatcher;
    private readonly ILogger<TelegramBotHostedService> _logger;

    public TelegramBotHostedService(
        IOptions<TelegramBotOptions> options,
        TelegramUpdateDispatcher dispatcher,
        ILogger<TelegramBotHostedService> logger)
    {
        _options = options.Value;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BotToken))
        {
            _logger.LogInformation("TelegramBot:BotToken is not configured — Telegram channel disabled.");
            return;
        }

        var bot = new TelegramBotClient(_options.BotToken);
        var me = await bot.GetMe(stoppingToken);
        _logger.LogInformation("Telegram bot started as @{Username}", me.Username);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
        };

        await bot.ReceiveAsync(
            async (client, update, ct) =>
            {
                try
                {
                    await _dispatcher.HandleAsync(client, update, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling Telegram update {UpdateId}", update.Id);
                }
            },
            async (client, exception, ct) =>
            {
                var message = exception is ApiRequestException apiEx
                    ? $"Telegram API error {apiEx.ErrorCode}: {apiEx.Message}"
                    : exception.ToString();
                _logger.LogError(exception, "Telegram polling error: {Message}", message);
                await Task.CompletedTask;
            },
            receiverOptions,
            stoppingToken);
    }
}
