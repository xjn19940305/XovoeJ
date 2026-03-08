using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetAndPaymentFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "orders",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_order_id",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_order_no",
                table: "orders",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "points_deduction_amount",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "points_used",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "refunded_amount",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "reward_points",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "wallet_pay_amount",
                table: "orders",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "payment_orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    payment_order_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    payable_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    refunded_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expire_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "points_accounts",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    available_points = table.Column<int>(type: "integer", nullable: false),
                    frozen_points = table.Column<int>(type: "integer", nullable: false),
                    total_earned_points = table.Column<int>(type: "integer", nullable: false),
                    total_spent_points = table.Column<int>(type: "integer", nullable: false),
                    last_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_points_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "points_logs",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    points_account_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    business_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    business_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    direction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    before_available_points = table.Column<int>(type: "integer", nullable: false),
                    before_frozen_points = table.Column<int>(type: "integer", nullable: false),
                    change_available_points = table.Column<int>(type: "integer", nullable: false),
                    change_frozen_points = table.Column<int>(type: "integer", nullable: false),
                    after_available_points = table.Column<int>(type: "integer", nullable: false),
                    after_frozen_points = table.Column<int>(type: "integer", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_points_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_accounts",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    available_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    frozen_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_income = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_expense = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    last_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    wallet_account_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    business_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    business_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    direction = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    before_available_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    before_frozen_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    change_available_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    change_frozen_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    after_available_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    after_frozen_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_transactions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_payment_order_no",
                table: "orders",
                column: "payment_order_no");

            migrationBuilder.CreateIndex(
                name: "IX_payment_orders_created_at",
                table: "payment_orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_payment_orders_expire_at",
                table: "payment_orders",
                column: "expire_at");

            migrationBuilder.CreateIndex(
                name: "IX_payment_orders_order_id",
                table: "payment_orders",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_orders_order_no",
                table: "payment_orders",
                column: "order_no");

            migrationBuilder.CreateIndex(
                name: "IX_payment_orders_payment_order_no",
                table: "payment_orders",
                column: "payment_order_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_orders_status",
                table: "payment_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_payment_orders_user_id",
                table: "payment_orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_points_accounts_created_at",
                table: "points_accounts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_points_accounts_last_changed_at",
                table: "points_accounts",
                column: "last_changed_at");

            migrationBuilder.CreateIndex(
                name: "IX_points_accounts_user_id",
                table: "points_accounts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_points_logs_business_no",
                table: "points_logs",
                column: "business_no");

            migrationBuilder.CreateIndex(
                name: "IX_points_logs_business_type",
                table: "points_logs",
                column: "business_type");

            migrationBuilder.CreateIndex(
                name: "IX_points_logs_created_at",
                table: "points_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_points_logs_idempotency_key",
                table: "points_logs",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_points_logs_points_account_id",
                table: "points_logs",
                column: "points_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_points_logs_user_id",
                table: "points_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_accounts_created_at",
                table: "wallet_accounts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_accounts_last_changed_at",
                table: "wallet_accounts",
                column: "last_changed_at");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_accounts_user_id",
                table: "wallet_accounts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_business_no",
                table: "wallet_transactions",
                column: "business_no");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_business_type",
                table: "wallet_transactions",
                column: "business_type");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_created_at",
                table: "wallet_transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_idempotency_key",
                table: "wallet_transactions",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_user_id",
                table: "wallet_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_wallet_account_id",
                table: "wallet_transactions",
                column: "wallet_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_orders");

            migrationBuilder.DropTable(
                name: "points_accounts");

            migrationBuilder.DropTable(
                name: "points_logs");

            migrationBuilder.DropTable(
                name: "wallet_accounts");

            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropIndex(
                name: "IX_orders_payment_order_no",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_order_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_order_no",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "points_deduction_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "points_used",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "refunded_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "reward_points",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "wallet_pay_amount",
                table: "orders");
        }
    }
}
