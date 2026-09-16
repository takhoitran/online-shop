using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.AiAdvisory.Domain;

namespace OnlineShop.Modules.AiAdvisory.Infrastructure.Persistence.Configurations;

internal sealed class AiInteractionLogConfiguration : IEntityTypeConfiguration<AiInteractionLog>
{
    public void Configure(EntityTypeBuilder<AiInteractionLog> builder)
    {
        builder.ToTable("ai_interaction_logs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.UserId).HasColumnName("user_id");
        builder.Property(l => l.Channel).HasColumnName("channel").HasMaxLength(20).IsRequired();
        builder.Property(l => l.Prompt).HasColumnName("prompt").IsRequired();
        builder.Property(l => l.Response).HasColumnName("response").IsRequired();
        builder.Property(l => l.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(l => l.UserId);
        builder.HasIndex(l => l.CreatedAtUtc);
    }
}
