using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Identity.Domain.Events;

public sealed record UserRegisteredDomainEvent(Guid UserId, string Username, Guid RoleId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
