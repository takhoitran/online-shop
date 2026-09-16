using MediatR;

namespace OnlineShop.IntegrationTests;

/// <summary>Only used to satisfy AppDbContextBase's constructor when migrating a DbContext
/// standalone, outside the real DI container — Database.MigrateAsync() never touches
/// SaveChangesAsync/the publisher, so nothing here is ever actually invoked.</summary>
internal sealed class NullPublisher : IPublisher
{
    public static readonly NullPublisher Instance = new();

    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification => Task.CompletedTask;
}
