using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "referral_links",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    owner_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    owner_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    landing_path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    full_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    attribution_source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    campaign_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    click_count = table.Column<int>(type: "integer", nullable: false),
                    signup_count = table.Column<int>(type: "integer", nullable: false),
                    first_order_count = table.Column<int>(type: "integer", nullable: false),
                    reward_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expire_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_visit_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_links", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_referral_links_campaign_name",
                table: "referral_links",
                column: "campaign_name");

            migrationBuilder.CreateIndex(
                name: "IX_referral_links_channel",
                table: "referral_links",
                column: "channel");

            migrationBuilder.CreateIndex(
                name: "IX_referral_links_code",
                table: "referral_links",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_referral_links_created_at",
                table: "referral_links",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_referral_links_expire_at",
                table: "referral_links",
                column: "expire_at");

            migrationBuilder.CreateIndex(
                name: "IX_referral_links_owner_id",
                table: "referral_links",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_referral_links_status",
                table: "referral_links",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referral_links");
        }
    }
}
