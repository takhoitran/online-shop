using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVouchersAndOrderDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "discount_currency",
                schema: "ordering",
                table: "orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "VND");

            migrationBuilder.AddColumn<decimal>(
                name: "subtotal_amount",
                schema: "ordering",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "subtotal_currency",
                schema: "ordering",
                table: "orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "VND");

            migrationBuilder.AddColumn<string>(
                name: "voucher_code",
                schema: "ordering",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Pre-existing orders had no discount concept — their subtotal simply equals what
            // was already charged as the total.
            migrationBuilder.Sql(
                "UPDATE ordering.orders SET subtotal_amount = total_amount, subtotal_currency = currency;");

            migrationBuilder.CreateTable(
                name: "vouchers",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    discount_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    max_uses = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vouchers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vouchers_code",
                schema: "ordering",
                table: "vouchers",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vouchers",
                schema: "ordering");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount_currency",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "subtotal_amount",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "subtotal_currency",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "voucher_code",
                schema: "ordering",
                table: "orders");
        }
    }
}
