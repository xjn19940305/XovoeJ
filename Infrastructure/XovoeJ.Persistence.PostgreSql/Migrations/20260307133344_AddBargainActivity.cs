using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddBargainActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bargain_activities",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    sku_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    original_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    floor_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    current_lowest_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    purchase_limit = table.Column<int>(type: "integer", nullable: false),
                    participant_count = table.Column<int>(type: "integer", nullable: false),
                    helper_count = table.Column<int>(type: "integer", nullable: false),
                    success_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bargain_activities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bargain_activities_code",
                table: "bargain_activities",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bargain_activities_created_at",
                table: "bargain_activities",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_bargain_activities_end_time",
                table: "bargain_activities",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_bargain_activities_start_time",
                table: "bargain_activities",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_bargain_activities_status",
                table: "bargain_activities",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bargain_activities");
        }
    }
}
