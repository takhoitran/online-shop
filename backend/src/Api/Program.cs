using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.BuildingBlocks.Application;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Modules.Catalog.Infrastructure.DependencyInjection;
using OnlineShop.Modules.Catalog.Infrastructure.Persistence;
using OnlineShop.Modules.Catalog.Infrastructure.Storage;
using OnlineShop.Modules.Identity.Infrastructure.DependencyInjection;
using OnlineShop.Modules.Identity.Infrastructure.Persistence;
using OnlineShop.Modules.Identity.Infrastructure.Seeding;
using OnlineShop.Modules.Inventory.Infrastructure.DependencyInjection;
using OnlineShop.Modules.Inventory.Infrastructure.Persistence;
using OnlineShop.Modules.Ordering.Infrastructure.DependencyInjection;
using OnlineShop.Modules.Ordering.Infrastructure.Persistence;
using OnlineShop.Modules.AiAdvisory.Infrastructure.DependencyInjection;
using OnlineShop.Modules.AiAdvisory.Infrastructure.Persistence;
using OnlineShop.Modules.Notification.Infrastructure.DependencyInjection;
using OnlineShop.Modules.Notification.Infrastructure.Persistence;
using OnlineShop.TelegramBot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddControllers();

// ---- CORS ----
// The JWT is sent via the Authorization header (never a cookie), so no AllowCredentials() is
// needed here — restricting to a configured origin allowlist is purely to stop some other site's
// JS from calling this Api on a visitor's behalf, not a session-hijack concern.
const string CorsPolicyName = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---- Modules ----
// Identity (Phase 1), Catalog (Phase 2), Inventory (Phase 3), and Ordering (Phase 4) are wired up
// so far. AiAdvisory/Notification join this list — AddXModule(...) + their Application assembly
// below — as each phase lands.
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddOrderingModule(builder.Configuration);
builder.Services.AddAiAdvisoryModule(builder.Configuration);
builder.Services.AddNotificationModule(builder.Configuration);
// Runs as a BackgroundService inside this same process — see TelegramBotHostedService's XML
// doc for why (one Modular Monolith process, two entry points, one DI container). No-ops if
// TelegramBot:BotToken is unset in appsettings (e.g. this machine has none configured yet).
builder.Services.AddTelegramBotModule(builder.Configuration);

// FileStorageOptions.RootPath comes from appsettings as a path relative to the Api's content
// root ("wwwroot/uploads/products") — resolve it to an absolute path here, once, at the
// composition root, so Catalog.Infrastructure never has to know about IHostEnvironment.
builder.Services.PostConfigure<FileStorageOptions>(options =>
    options.RootPath = Path.Combine(builder.Environment.ContentRootPath, options.RootPath));

var moduleApplicationAssemblies = new[]
{
    typeof(OnlineShop.Modules.Identity.Application.Register.RegisterUserCommand).Assembly,
    typeof(OnlineShop.Modules.Catalog.Application.Products.CreateProductCommand).Assembly,
    typeof(OnlineShop.Modules.Inventory.Application.ReceiveStock.ReceiveStockCommand).Assembly,
    typeof(OnlineShop.Modules.Ordering.Application.Orders.CheckoutCommand).Assembly,
    typeof(OnlineShop.Modules.AiAdvisory.Application.Advisory.AskProductAdvisorCommand).Assembly,
    typeof(OnlineShop.Modules.Notification.Application.Notifications.GetNotificationsQuery).Assembly
};

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(moduleApplicationAssemblies);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

foreach (var assembly in moduleApplicationAssemblies)
    builder.Services.AddValidatorsFromAssembly(assembly);

// ---- Auth ----
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    // Admin has every Seller capability plus user management — see AdminUsersController.
    .AddPolicy("SellerOrAdmin", policy => policy.RequireRole("Seller", "Admin"))
    .AddPolicy("BuyerOnly", policy => policy.RequireRole("Buyer"));

// ---- Rate limiting ----
// AI Advisory calls out to the real Gemini API on every request — unmetered, one abusive client
// (or a buggy frontend retry loop) could burn a day's quota in seconds. Partitioned by user id
// when authenticated, falling back to IP for anonymous chat callers (AskProductAdvisor allows
// Guests) — a shared global limiter would let one heavy user starve everyone else.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("ai", httpContext =>
    {
        var key = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    // Login is always anonymous (no user id to partition by yet) — partition by caller IP to
    // slow down password-guessing without needing a CAPTCHA or account lockout for this project's
    // scope. A shared global limiter would let one attacker's traffic block every real user too.
    options.AddPolicy("login", httpContext =>
    {
        var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

var app = builder.Build();

// Translate FluentValidation failures and domain rule violations into a clean 400 response
// instead of a raw 500, without adding a bespoke exception type per module.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    var problem = feature?.Error switch
    {
        ValidationException validationEx => new ProblemDetails
        {
            Title = "Validation.Failed",
            Detail = string.Join(" | ", validationEx.Errors.Select(e => e.ErrorMessage)),
            Status = StatusCodes.Status400BadRequest
        },
        OnlineShop.BuildingBlocks.Domain.DomainException domainEx => new ProblemDetails
        {
            Title = "Domain.RuleViolation",
            Detail = domainEx.Message,
            Status = StatusCodes.Status400BadRequest
        },
        OnlineShop.Modules.AiAdvisory.Application.Abstractions.AiServiceUnavailableException aiEx => new ProblemDetails
        {
            Title = "Ai.ServiceUnavailable",
            Detail = aiEx.Message,
            Status = StatusCodes.Status503ServiceUnavailable
        },
        _ => new ProblemDetails
        {
            Title = "Server.Error",
            Detail = "Something went wrong, please try again later.",
            Status = StatusCodes.Status500InternalServerError
        }
    };

    context.Response.StatusCode = problem.Status!.Value;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(problem);
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

// Apply every module's pending migrations on startup — without this, a fresh Postgres volume
// (e.g. a first `docker compose up` on another machine, or CI) has no schema at all and every
// request 500s with "relation does not exist". Each module owns its own migrations/schema, so
// each DbContext is migrated independently rather than assuming a shared one.
using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    await provider.GetRequiredService<IdentityDbContext>().Database.MigrateAsync();
    await provider.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
    await provider.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
    await provider.GetRequiredService<OrderingDbContext>().Database.MigrateAsync();
    await provider.GetRequiredService<AiAdvisoryDbContext>().Database.MigrateAsync();
    await provider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();

// Top-level statements make Program implicitly internal — this partial re-declaration exposes it
// so WebApplicationFactory<Program> (integration tests, a separate assembly) can reference it.
public partial class Program { }
