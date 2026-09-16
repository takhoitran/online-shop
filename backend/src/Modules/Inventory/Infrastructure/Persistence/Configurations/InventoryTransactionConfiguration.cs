using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Modules.Inventory.Domain;

namespace OnlineShop.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("inventory_transactions");
        builder.HasKey(t => t.Id);

        // InventoryTransaction is only ever reached via ProductStock's private collection (never
        // repository.Add()'d directly), so EF discovers new rows through graph fixup during
        // DetectChanges rather than an explicit Add(). Without ValueGeneratedNever(), EF's default
        // convention for a client-generated Guid key makes it assume such a "never seen before but
        // non-default key" entity already exists in the database, and issues a no-op UPDATE
        // instead of an INSERT (which then throws DbUpdateConcurrencyException: 0 rows affected).
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Type)
            .HasColumnName("transaction_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(t => t.OrderId).HasColumnName("order_id");
        builder.Property(t => t.PerformedByUserId).HasColumnName("performed_by_user_id").IsRequired();
        builder.Property(t => t.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(t => t.OccurredAtUtc).HasColumnName("occurred_at_utc");

        builder.Property<Guid>("ProductStockId").HasColumnName("product_stock_id");
        builder.HasIndex("ProductStockId");
    }
}
