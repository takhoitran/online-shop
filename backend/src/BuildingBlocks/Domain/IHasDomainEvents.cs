namespace OnlineShop.BuildingBlocks.Domain;

/// <summary>Non-generic marker so infrastructure code (DbContext.ChangeTracker) can find every
/// Aggregate Root regardless of its TId, without depending on EF Core from the Domain layer.</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
