using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OnlineShop.TelegramBot.DependencyInjection;

public static class TelegramBotModule
{
    public static IServiceCollection AddTelegramBotModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));

        // Singleton, not scoped: it only holds an IServiceScopeFactory and opens its own scope
        // per Update internally (see HandleAsync) — the same shape as MediatR's own registration,
        // safe to inject into the singleton BackgroundService below.
        services.AddSingleton<TelegramUpdateDispatcher>();
        services.AddHostedService<TelegramBotHostedService>();
        return services;
    }
}
