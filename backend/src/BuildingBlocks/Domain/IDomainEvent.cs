using MediatR;

namespace OnlineShop.BuildingBlocks.Domain;

/// <summary>
/// Marker for something that happened inside an Aggregate. Raised via AggregateRoot.Raise()
/// and dispatched as a MediatR notification from inside SaveChangesAsync — same DB transaction,
/// no outbox needed while everything runs in-process (Modular Monolith). See EfUnitOfWork.
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
