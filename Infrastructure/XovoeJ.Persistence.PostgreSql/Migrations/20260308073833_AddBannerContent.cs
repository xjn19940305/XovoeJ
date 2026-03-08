using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    link_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banners", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_banners_created_at",
                table: "banners",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_banners_end_time",
                table: "banners",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_banners_is_enabled",
                table: "banners",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_banners_sort_order",
                table: "banners",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "IX_banners_start_time",
                table: "banners",
                column: "start_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banners");
        }
    }
}
