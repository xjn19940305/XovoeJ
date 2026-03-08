using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoice_email",
                table: "orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_tax_no",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_title",
                table: "orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "invoice_type",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "need_invoice",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoice_email",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "invoice_tax_no",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "invoice_title",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "invoice_type",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "need_invoice",
                table: "orders");
        }
    }
}
