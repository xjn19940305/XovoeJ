using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class adddictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dictionary_groups",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    parent_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    icon = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dictionary_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_dictionary_groups_dictionary_groups_parent_id",
                        column: x => x.parent_id,
                        principalTable: "dictionary_groups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "dictionary_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    group_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    data_type = table.Column<int>(type: "integer", nullable: false),
                    default_value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    placeholder = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    options = table.Column<string>(type: "jsonb", nullable: true),
                    validation = table.Column<string>(type: "jsonb", nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dictionary_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_dictionary_items_dictionary_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "dictionary_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_groups_code",
                table: "dictionary_groups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_groups_parent_id",
                table: "dictionary_groups",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_groups_path",
                table: "dictionary_groups",
                column: "path");

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_groups_type",
                table: "dictionary_groups",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_items_group_id",
                table: "dictionary_items",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_items_is_enabled",
                table: "dictionary_items",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_items_key",
                table: "dictionary_items",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dictionary_items");

            migrationBuilder.DropTable(
                name: "dictionary_groups");
        }
    }
}
