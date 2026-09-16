using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.BuildingBlocks.Infrastructure;
using OnlineShop.Modules.Notification.Application.Abstractions;
using OnlineShop.Modules.Notification.Domain;

namespace OnlineShop.Modules.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext : AppDbContextBase, INotificationUnitOfWork
{
    public const string Schema = "notification";

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options, IPublisher publisher)
        : base(options, publisher)
    {
    }

    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
