using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.AiAdvisory.Domain;

/// <summary>Audit trail of every AI exchange (buyer product-advisor chat, seller restock report)
/// — not a business aggregate with invariants of its own, just an append-only log for the
/// mentor/auditor to review later. See the plan's Phase 8 design note on Ai_Interaction_Logs.</summary>
public sealed class AiInteractionLog : Entity<Guid>
{
    public Guid? UserId { get; private set; }
    public string Channel { get; private set; } = default!;
    public string Prompt { get; private set; } = default!;
    public string Response { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }

    private AiInteractionLog() { }

    private AiInteractionLog(Guid id, Guid? userId, string channel, string prompt, string response, DateTime createdAtUtc) : base(id)
    {
        UserId = userId;
        Channel = channel;
        Prompt = prompt;
        Response = response;
        CreatedAtUtc = createdAtUtc;
    }

    public static AiInteractionLog Create(Guid? userId, string channel, string prompt, string response)
    {
        Guard.AgainstNullOrWhiteSpace(channel, nameof(channel));
        Guard.AgainstNullOrWhiteSpace(prompt, nameof(prompt));

        return new AiInteractionLog(Guid.NewGuid(), userId, channel, prompt, response ?? string.Empty, DateTime.UtcNow);
    }
}
