using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Identity.Domain.Events;

public sealed record TelegramAccountLinkedDomainEvent(Guid UserId, string TelegramId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
