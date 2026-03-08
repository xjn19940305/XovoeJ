using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketingCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coupon_templates",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    coupon_type = table.Column<int>(type: "integer", nullable: false),
                    discount_type = table.Column<int>(type: "integer", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    min_order_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_quantity = table.Column<int>(type: "integer", nullable: false),
                    issued_quantity = table.Column<int>(type: "integer", nullable: false),
                    used_quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    receive_limit = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotion_activities",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    scope_text = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    stackable = table.Column<bool>(type: "boolean", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    participant_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_activities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coupon_templates_code",
                table: "coupon_templates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coupon_templates_coupon_type",
                table: "coupon_templates",
                column: "coupon_type");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_templates_created_at",
                table: "coupon_templates",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_templates_end_time",
                table: "coupon_templates",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_templates_start_time",
                table: "coupon_templates",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_templates_status",
                table: "coupon_templates",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_activities_created_at",
                table: "promotion_activities",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_activities_end_time",
                table: "promotion_activities",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_activities_priority",
                table: "promotion_activities",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_activities_start_time",
                table: "promotion_activities",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_activities_status",
                table: "promotion_activities",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_activities_type",
                table: "promotion_activities",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coupon_templates");

            migrationBuilder.DropTable(
                name: "promotion_activities");
        }
    }
}
