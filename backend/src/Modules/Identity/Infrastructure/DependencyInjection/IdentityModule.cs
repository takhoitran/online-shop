using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Modules.Identity.Application.Abstractions;
using OnlineShop.Modules.Identity.Domain;
using OnlineShop.Modules.Identity.Infrastructure.Persistence;
using OnlineShop.Modules.Identity.Infrastructure.Persistence.Queries;
using OnlineShop.Modules.Identity.Infrastructure.Persistence.Repositories;
using OnlineShop.Modules.Identity.Infrastructure.Security;

namespace OnlineShop.Modules.Identity.Infrastructure.DependencyInjection;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(
            configuration.GetConnectionString("Postgres"),
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", IdentityDbContext.Schema)));

        services.AddScoped<IIdentityUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ITelegramLinkCodeRepository, TelegramLinkCodeRepository>();
        services.AddScoped<IUserDirectoryQueries, UserDirectoryQueries>();

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
