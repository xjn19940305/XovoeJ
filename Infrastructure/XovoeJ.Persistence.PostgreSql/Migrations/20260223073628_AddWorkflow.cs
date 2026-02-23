using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflow_approval_records",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    instance_id = table.Column<string>(type: "text", nullable: false),
                    step_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    step_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    approver_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    approver_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    action = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    attachments = table.Column<string>(type: "jsonb", nullable: true),
                    action_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_approval_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definitions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    steps_config = table.Column<string>(type: "jsonb", nullable: true),
                    form_config = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_instances",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    workflow_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    workflow_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    initiator_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    initiator_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    business_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    business_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    current_step_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    current_step_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    form_data = table.Column<string>(type: "jsonb", nullable: true),
                    variables = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_instances", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_pending_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    instance_id = table.Column<string>(type: "text", nullable: false),
                    step_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    step_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    approver_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_pending_items", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_approval_records_action_time",
                table: "workflow_approval_records",
                column: "action_time");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_approval_records_approver_id",
                table: "workflow_approval_records",
                column: "approver_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_approval_records_instance_id",
                table: "workflow_approval_records",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_approval_records_step_id",
                table: "workflow_approval_records",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definitions_code",
                table: "workflow_definitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definitions_is_enabled",
                table: "workflow_definitions",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definitions_type",
                table: "workflow_definitions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_business_key",
                table: "workflow_instances",
                column: "business_key");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_created_at",
                table: "workflow_instances",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_initiator_id",
                table: "workflow_instances",
                column: "initiator_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_status",
                table: "workflow_instances",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instances_workflow_code",
                table: "workflow_instances",
                column: "workflow_code");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_pending_items_approver_id",
                table: "workflow_pending_items",
                column: "approver_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_pending_items_instance_id",
                table: "workflow_pending_items",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_pending_items_instance_id_approver_id",
                table: "workflow_pending_items",
                columns: new[] { "instance_id", "approver_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_approval_records");

            migrationBuilder.DropTable(
                name: "workflow_definitions");

            migrationBuilder.DropTable(
                name: "workflow_instances");

            migrationBuilder.DropTable(
                name: "workflow_pending_items");
        }
    }
}
