using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddAfterSaleOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "after_sale_orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    after_sale_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    contact_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    contact_mobile = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    original_order_status = table.Column<int>(type: "integer", nullable: false),
                    evidence_images = table.Column<string>(type: "text", nullable: true),
                    admin_remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    applied_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    audited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refunded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_after_sale_orders", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_after_sale_orders_after_sale_no",
                table: "after_sale_orders",
                column: "after_sale_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_after_sale_orders_created_at",
                table: "after_sale_orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_after_sale_orders_order_id",
                table: "after_sale_orders",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_after_sale_orders_order_no",
                table: "after_sale_orders",
                column: "order_no");

            migrationBuilder.CreateIndex(
                name: "IX_after_sale_orders_status",
                table: "after_sale_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_after_sale_orders_type",
                table: "after_sale_orders",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_after_sale_orders_user_id",
                table: "after_sale_orders",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "after_sale_orders");
        }
    }
}
