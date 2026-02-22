using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XovoeJ.Persistence.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class ShopPArt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    parent_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    icon = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    image = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_categories_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    order_no = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    freight_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    pay_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    pay_status = table.Column<int>(type: "integer", nullable: false),
                    pay_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ship_status = table.Column<int>(type: "integer", nullable: false),
                    ship_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    receive_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finish_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancel_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    consignee_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    consignee_mobile = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    consignee_address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "spec_groups",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spec_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    subtitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    main_image = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    images = table.Column<string>(type: "jsonb", nullable: true),
                    detail = table.Column<string>(type: "text", nullable: true),
                    sales_count = table.Column<int>(type: "integer", nullable: false),
                    browse_count = table.Column<int>(type: "integer", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_hot = table.Column<bool>(type: "boolean", nullable: false),
                    is_new = table.Column<bool>(type: "boolean", nullable: false),
                    is_recommend = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    order_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    product_image = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    sku_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sku_specs = table.Column<string>(type: "jsonb", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spec_values",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    spec_group_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    value = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "product_skus",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    product_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sku_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    specs = table.Column<string>(type: "jsonb", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    original_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    cost_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    low_stock = table.Column<int>(type: "integer", nullable: false),
                    sales_count = table.Column<int>(type: "integer", nullable: false),
                    image = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_skus", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_skus_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shopping_cart",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    product_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sku_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_cart", x => x.id);
                    table.ForeignKey(
                        name: "FK_shopping_cart_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_parent_id",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_categories_path",
                table: "categories",
                column: "path");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_no",
                table: "orders",
                column: "order_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_skus_product_id",
                table: "product_skus",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_skus_sku_code",
                table: "product_skus",
                column: "sku_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_skus_stock",
                table: "product_skus",
                column: "stock");

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_enabled",
                table: "products",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_hot",
                table: "products",
                column: "is_hot");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_new",
                table: "products",
                column: "is_new");

            migrationBuilder.CreateIndex(
                name: "IX_products_is_recommend",
                table: "products",
                column: "is_recommend");

            migrationBuilder.CreateIndex(
                name: "IX_products_sales_count",
                table: "products",
                column: "sales_count");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_product_id",
                table: "shopping_cart",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_sku_id",
                table: "shopping_cart",
                column: "sku_id");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_user_id",
                table: "shopping_cart",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_user_id_sku_id",
                table: "shopping_cart",
                columns: new[] { "user_id", "sku_id" },
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "product_skus");

            migrationBuilder.DropTable(
                name: "shopping_cart");

            migrationBuilder.DropTable(
                name: "spec_values");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "spec_groups");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
