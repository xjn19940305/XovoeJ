using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddGrowthCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "commission_records",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    promoter_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    promoter_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    order_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    rule_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    source_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    estimated_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    settled_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    settled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commission_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invite_relations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    inviter_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    invitee_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    inviter_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    invitee_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    referral_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    attribution_source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    total_orders = table.Column<int>(type: "integer", nullable: false),
                    total_reward_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    invited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    first_order_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invite_relations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commission_records_created_at",
                table: "commission_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_commission_records_order_no",
                table: "commission_records",
                column: "order_no");

            migrationBuilder.CreateIndex(
                name: "IX_commission_records_promoter_id",
                table: "commission_records",
                column: "promoter_id");

            migrationBuilder.CreateIndex(
                name: "IX_commission_records_settled_at",
                table: "commission_records",
                column: "settled_at");

            migrationBuilder.CreateIndex(
                name: "IX_commission_records_status",
                table: "commission_records",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_invite_relations_channel",
                table: "invite_relations",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "IX_invite_relations_created_at",
                table: "invite_relations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_invite_relations_invitee_id",
                table: "invite_relations",
                column: "invitee_id");

            migrationBuilder.CreateIndex(
                name: "IX_invite_relations_inviter_id",
                table: "invite_relations",
                column: "inviter_id");

            migrationBuilder.CreateIndex(
                name: "IX_invite_relations_referral_code",
                table: "invite_relations",
                column: "referral_code");

            migrationBuilder.CreateIndex(
                name: "IX_invite_relations_status",
                table: "invite_relations",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commission_records");

            migrationBuilder.DropTable(
                name: "invite_relations");
        }
    }
}
