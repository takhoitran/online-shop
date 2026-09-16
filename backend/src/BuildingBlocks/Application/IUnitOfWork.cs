namespace OnlineShop.BuildingBlocks.Application;

/// <summary>
/// Each module owns its own DbContext/schema and implements this once. SaveChangesAsync
/// must dispatch the aggregates' collected domain events (as MediatR notifications) inside
/// the same transaction, then clear them — see EfUnitOfWork in each module's Infrastructure.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
