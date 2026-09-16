using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Modules.AiAdvisory.Application.Abstractions;
using OnlineShop.Modules.AiAdvisory.Infrastructure.Gemini;
using OnlineShop.Modules.AiAdvisory.Infrastructure.Persistence;
using OnlineShop.Modules.AiAdvisory.Infrastructure.Persistence.Repositories;

namespace OnlineShop.Modules.AiAdvisory.Infrastructure.DependencyInjection;

public static class AiAdvisoryModule
{
    public static IServiceCollection AddAiAdvisoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AiAdvisoryDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Postgres"),
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", AiAdvisoryDbContext.Schema)));

        services.AddScoped<IAiAdvisoryUnitOfWork>(sp => sp.GetRequiredService<AiAdvisoryDbContext>());
        services.AddScoped<IAiInteractionLogRepository, AiInteractionLogRepository>();

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.AddHttpClient<IGenerativeAiClient, GeminiClient>(client =>
        {
            client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
