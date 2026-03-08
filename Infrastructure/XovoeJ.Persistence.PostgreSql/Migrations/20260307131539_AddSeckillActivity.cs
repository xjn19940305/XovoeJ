using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddSeckillActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "seckill_activities",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    sku_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    cover_image = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    original_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    seckill_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_stock = table.Column<int>(type: "integer", nullable: false),
                    locked_stock = table.Column<int>(type: "integer", nullable: false),
                    sold_stock = table.Column<int>(type: "integer", nullable: false),
                    purchase_limit = table.Column<int>(type: "integer", nullable: false),
                    participant_count = table.Column<int>(type: "integer", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    warmup_start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seckill_activities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_seckill_activities_code",
                table: "seckill_activities",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seckill_activities_created_at",
                table: "seckill_activities",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_seckill_activities_end_time",
                table: "seckill_activities",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_seckill_activities_start_time",
                table: "seckill_activities",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_seckill_activities_status",
                table: "seckill_activities",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "seckill_activities");
        }
    }
}
