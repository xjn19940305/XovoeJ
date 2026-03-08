using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupBuyActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "group_buy_activities",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    sku_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    original_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    group_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    group_size = table.Column<int>(type: "integer", nullable: false),
                    purchase_limit = table.Column<int>(type: "integer", nullable: false),
                    virtual_group_count = table.Column<int>(type: "integer", nullable: false),
                    participant_count = table.Column<int>(type: "integer", nullable: false),
                    success_group_count = table.Column<int>(type: "integer", nullable: false),
                    failed_group_count = table.Column<int>(type: "integer", nullable: false),
                    group_leader_reward = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_buy_activities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_group_buy_activities_code",
                table: "group_buy_activities",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_buy_activities_created_at",
                table: "group_buy_activities",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_group_buy_activities_end_time",
                table: "group_buy_activities",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_group_buy_activities_start_time",
                table: "group_buy_activities",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_group_buy_activities_status",
                table: "group_buy_activities",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_buy_activities");
        }
    }
}
