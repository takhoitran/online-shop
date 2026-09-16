using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OnlineShop.Modules.AiAdvisory.Infrastructure.Persistence;
using OnlineShop.Modules.Catalog.Infrastructure.Persistence;
using OnlineShop.Modules.Identity.Infrastructure.Persistence;
using OnlineShop.Modules.Inventory.Infrastructure.Persistence;
using OnlineShop.Modules.Notification.Infrastructure.Persistence;
using OnlineShop.Modules.Ordering.Infrastructure.Persistence;

namespace OnlineShop.IntegrationTests;

/// <summary>
/// Runs the real Api host (all modules, real MediatR pipeline) against a dedicated
/// "onlineshop_test" Postgres database on the same local dev server (docker-compose, port 5433)
/// — never the "onlineshop" dev database these tests would otherwise pollute. Creates the test
/// database and applies every module's migrations once per test run; Gemini/Telegram tokens are
/// deliberately left unconfigured (empty), matching how those modules already no-op gracefully
/// without real credentials — no live external calls happen from this suite.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string MaintenanceConnectionString = "Host=localhost;Port=5433;Database=postgres;Username=onlineshop;Password=onlineshop_dev_password";
    public const string TestConnectionString = "Host=localhost;Port=5433;Database=onlineshop_test;Username=onlineshop;Password=onlineshop_dev_password";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = TestConnectionString,
            });
        });
    }

    public async Task InitializeAsync()
    {
        await EnsureTestDatabaseExistsAsync();

        // Deliberately NOT done via `Services.CreateScope()` — the first touch of this factory's
        // `Services` property boots the real host, and Program.cs runs IdentitySeeder.SeedAsync
        // inline before returning control, which would query "identity.users" before any
        // migration below has had a chance to create it. Each DbContext is migrated standalone
        // here instead, fully independent of the WebApplicationFactory's own host lifecycle.
        await MigrateAsync(() => new IdentityDbContext(BuildOptions<IdentityDbContext>(IdentityDbContext.Schema), NullPublisher.Instance));
        await MigrateAsync(() => new CatalogDbContext(BuildOptions<CatalogDbContext>(CatalogDbContext.Schema), NullPublisher.Instance));
        await MigrateAsync(() => new InventoryDbContext(BuildOptions<InventoryDbContext>(InventoryDbContext.Schema), NullPublisher.Instance));
        await MigrateAsync(() => new OrderingDbContext(BuildOptions<OrderingDbContext>(OrderingDbContext.Schema), NullPublisher.Instance));
        await MigrateAsync(() => new AiAdvisoryDbContext(BuildOptions<AiAdvisoryDbContext>(AiAdvisoryDbContext.Schema), NullPublisher.Instance));
        await MigrateAsync(() => new NotificationDbContext(BuildOptions<NotificationDbContext>(NotificationDbContext.Schema), NullPublisher.Instance));
    }

    // Must match each module's real DI registration (AddXModule -> MigrationsHistoryTable(...,
    // Schema)) exactly, or this standalone pre-migration and the app's own DI-resolved DbContext
    // disagree on which table records "what's already applied" — the app then thinks nothing has
    // run yet and tries to re-create tables that already exist ("relation already exists").
    private static DbContextOptions<TContext> BuildOptions<TContext>(string schema) where TContext : DbContext =>
        new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(TestConnectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", schema))
            .Options;

    private static async Task MigrateAsync<TContext>(Func<TContext> factory) where TContext : DbContext
    {
        await using var context = factory();
        await context.Database.MigrateAsync();
    }

    // Explicit implementation: WebApplicationFactory<T> already exposes its own public
    // IAsyncDisposable.DisposeAsync() (returns ValueTask) — a same-named public Task-returning
    // method would clash with it, so xUnit's IAsyncLifetime member is implemented explicitly.
    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private static async Task EnsureTestDatabaseExistsAsync()
    {
        await using var connection = new NpgsqlConnection(MaintenanceConnectionString);
        await connection.OpenAsync();

        await using (var checkCommand = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'onlineshop_test'", connection))
        {
            var exists = await checkCommand.ExecuteScalarAsync() is not null;
            if (exists)
                return;
        }

        await using var createCommand = new NpgsqlCommand("CREATE DATABASE onlineshop_test", connection);
        await createCommand.ExecuteNonQueryAsync();
    }
}
