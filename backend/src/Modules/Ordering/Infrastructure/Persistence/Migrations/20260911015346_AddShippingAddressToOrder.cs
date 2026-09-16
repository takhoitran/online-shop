using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineShop.Modules.Ordering.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingAddressToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "shipping_address_line",
                schema: "ordering",
                table: "orders",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "(not provided — order placed before shipping address was collected)");

            migrationBuilder.AddColumn<string>(
                name: "shipping_city",
                schema: "ordering",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<string>(
                name: "shipping_phone_number",
                schema: "ordering",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<string>(
                name: "shipping_recipient_name",
                schema: "ordering",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "Unknown");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shipping_address_line",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_city",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_phone_number",
                schema: "ordering",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_recipient_name",
                schema: "ordering",
                table: "orders");
        }
    }
}
