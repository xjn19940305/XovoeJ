using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddMallUserAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_addresses",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    consignee_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    mobile = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    province = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    city = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    area = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    region_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    detail_address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    label = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_addresses", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_addresses_created_at",
                table: "user_addresses",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_addresses_region_code",
                table: "user_addresses",
                column: "region_code");

            migrationBuilder.CreateIndex(
                name: "IX_user_addresses_user_id",
                table: "user_addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_addresses_user_id_is_default",
                table: "user_addresses",
                columns: new[] { "user_id", "is_default" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_addresses");
        }
    }
}
