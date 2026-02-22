using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class removesomeskutables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "spec_values");

            migrationBuilder.DropTable(
                name: "spec_groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "spec_groups",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spec_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spec_values",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    spec_group_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spec_values", x => x.id);
                    table.ForeignKey(
                        name: "FK_spec_values_spec_groups_spec_group_id",
                        column: x => x.spec_group_id,
                        principalTable: "spec_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_spec_groups_sort_order",
                table: "spec_groups",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "IX_spec_values_sort_order",
                table: "spec_values",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "IX_spec_values_spec_group_id",
                table: "spec_values",
                column: "spec_group_id");
        }
    }
}
