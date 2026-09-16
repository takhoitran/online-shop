using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Modules.Notification.Application.Abstractions;
using OnlineShop.Modules.Notification.Infrastructure.Persistence;
using OnlineShop.Modules.Notification.Infrastructure.Persistence.Queries;
using OnlineShop.Modules.Notification.Infrastructure.Persistence.Repositories;
using OnlineShop.Modules.Notification.Infrastructure.Telegram;

namespace OnlineShop.Modules.Notification.Infrastructure.DependencyInjection;

public static class NotificationModule
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Postgres"),
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", NotificationDbContext.Schema)));

        services.AddScoped<INotificationUnitOfWork>(sp => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<IAppNotificationRepository, AppNotificationRepository>();
        services.AddScoped<INotificationQueries, NotificationQueries>();

        services.Configure<TelegramNotifierOptions>(configuration.GetSection(TelegramNotifierOptions.SectionName));
        services.AddSingleton<ITelegramNotifier, TelegramNotifier>();

        return services;
    }
}
