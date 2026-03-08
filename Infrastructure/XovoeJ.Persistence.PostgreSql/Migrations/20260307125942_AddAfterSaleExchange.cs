using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddAfterSaleExchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "exchange_remark",
                table: "after_sale_orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "exchange_shipping_company",
                table: "after_sale_orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "exchange_tracking_no",
                table: "after_sale_orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "exchanged_at",
                table: "after_sale_orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exchange_remark",
                table: "after_sale_orders");

            migrationBuilder.DropColumn(
                name: "exchange_shipping_company",
                table: "after_sale_orders");

            migrationBuilder.DropColumn(
                name: "exchange_tracking_no",
                table: "after_sale_orders");

            migrationBuilder.DropColumn(
                name: "exchanged_at",
                table: "after_sale_orders");
        }
    }
}
