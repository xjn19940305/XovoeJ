using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberCouponLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "coupon_name",
                table: "orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "coupon_template_id",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_coupon_id",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentMemberLevelCode",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RewardedMemberLevelCode",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSpentAmount",
                table: "AspNetUsers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "coupon_issue_batches",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    target_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    coupon_template_ids_json = table.Column<string>(type: "text", nullable: false),
                    target_user_ids_json = table.Column<string>(type: "text", nullable: true),
                    target_member_levels_json = table.Column<string>(type: "text", nullable: true),
                    requested_count = table.Column<int>(type: "integer", nullable: false),
                    succeeded_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_issue_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "coupon_issue_records",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    batch_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    coupon_template_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_coupon_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    error_message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_issue_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "member_level_reward_rules",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    level_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    level_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    coupon_template_ids_json = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member_level_reward_rules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_coupons",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    coupon_template_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    source_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    snapshot_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    snapshot_coupon_type = table.Column<int>(type: "integer", nullable: false),
                    snapshot_discount_type = table.Column<int>(type: "integer", nullable: false),
                    snapshot_discount_value = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    snapshot_min_order_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    claimed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    order_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    order_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_coupons", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_batches_created_at",
                table: "coupon_issue_batches",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_batches_status",
                table: "coupon_issue_batches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_batches_target_type",
                table: "coupon_issue_batches",
                column: "target_type");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_records_batch_id",
                table: "coupon_issue_records",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_records_coupon_template_id",
                table: "coupon_issue_records",
                column: "coupon_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_records_created_at",
                table: "coupon_issue_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_records_user_coupon_id",
                table: "coupon_issue_records",
                column: "user_coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_issue_records_user_id",
                table: "coupon_issue_records",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_level_reward_rules_level_code",
                table: "member_level_reward_rules",
                column: "level_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_member_level_reward_rules_sort",
                table: "member_level_reward_rules",
                column: "sort");

            migrationBuilder.CreateIndex(
                name: "IX_member_level_reward_rules_status",
                table: "member_level_reward_rules",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_user_coupons_coupon_template_id",
                table: "user_coupons",
                column: "coupon_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_coupons_created_at",
                table: "user_coupons",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_coupons_order_id",
                table: "user_coupons",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_coupons_status",
                table: "user_coupons",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_user_coupons_user_id",
                table: "user_coupons",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_coupons_user_id_status",
                table: "user_coupons",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coupon_issue_batches");

            migrationBuilder.DropTable(
                name: "coupon_issue_records");

            migrationBuilder.DropTable(
                name: "member_level_reward_rules");

            migrationBuilder.DropTable(
                name: "user_coupons");

            migrationBuilder.DropColumn(
                name: "coupon_name",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "coupon_template_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "user_coupon_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "CurrentMemberLevelCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RewardedMemberLevelCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalSpentAmount",
                table: "AspNetUsers");
        }
    }
}
