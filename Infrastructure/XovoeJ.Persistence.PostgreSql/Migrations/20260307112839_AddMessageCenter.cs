using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_dictionary_items_key",
                table: "dictionary_items");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "message_templates",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    business_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    subject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    content_preview = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "message_tasks",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    template_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    trigger_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    recipient_count = table.Column<int>(type: "integer", nullable: false),
                    success_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_message_tasks_message_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "message_templates",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "message_send_records",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    template_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    task_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    recipient = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    business_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    trace_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_send_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_message_send_records_message_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "message_tasks",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_message_send_records_message_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "message_templates",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_is_deleted",
                table: "orders",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_items_group_id_key",
                table: "dictionary_items",
                columns: new[] { "group_id", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_message_send_records_channel",
                table: "message_send_records",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "IX_message_send_records_created_at",
                table: "message_send_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_message_send_records_sent_at",
                table: "message_send_records",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "IX_message_send_records_status",
                table: "message_send_records",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_message_send_records_task_id",
                table: "message_send_records",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_send_records_template_id",
                table: "message_send_records",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_send_records_trace_id",
                table: "message_send_records",
                column: "trace_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_tasks_channel",
                table: "message_tasks",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "IX_message_tasks_created_at",
                table: "message_tasks",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_message_tasks_scheduled_at",
                table: "message_tasks",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "IX_message_tasks_status",
                table: "message_tasks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_message_tasks_template_id",
                table: "message_tasks",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_templates_channel",
                table: "message_templates",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "IX_message_templates_code",
                table: "message_templates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_message_templates_created_at",
                table: "message_templates",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_message_templates_status",
                table: "message_templates",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_send_records");

            migrationBuilder.DropTable(
                name: "message_tasks");

            migrationBuilder.DropTable(
                name: "message_templates");

            migrationBuilder.DropIndex(
                name: "IX_orders_is_deleted",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_dictionary_items_group_id_key",
                table: "dictionary_items");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "orders");

            migrationBuilder.CreateIndex(
                name: "IX_dictionary_items_key",
                table: "dictionary_items",
                column: "key",
                unique: true);
        }
    }
}
