using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ResumeInOneMinute.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingAndMasterTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "master");

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.CreateTable(
                name: "master_values",
                schema: "master",
                columns: table => new
                {
                    master_value_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    master_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_master_values", x => x.master_value_id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                schema: "billing",
                columns: table => new
                {
                    plan_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plan_code_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_plans", x => x.plan_id);
                    table.ForeignKey(
                        name: "fk_subscription_plans_master_values_plan_code_id",
                        column: x => x.plan_code_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plan_prices",
                schema: "billing",
                columns: table => new
                {
                    plan_price_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    billing_cycle_id = table.Column<long>(type: "bigint", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_plan_prices", x => x.plan_price_id);
                    table.ForeignKey(
                        name: "fk_subscription_plan_prices_master_values_billing_cycle_id",
                        column: x => x.billing_cycle_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subscription_plan_prices_master_values_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subscription_plan_prices_subscription_plans_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "billing",
                        principalTable: "subscription_plans",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_subscriptions",
                schema: "billing",
                columns: table => new
                {
                    user_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    global_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    plan_price_id = table.Column<long>(type: "bigint", nullable: false),
                    status_id = table.Column<long>(type: "bigint", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    auto_renew = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_subscriptions", x => x.user_subscription_id);
                    table.ForeignKey(
                        name: "fk_user_subscriptions_master_values_status_id",
                        column: x => x.status_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_subscriptions_subscription_plan_prices_plan_price_id",
                        column: x => x.plan_price_id,
                        principalSchema: "billing",
                        principalTable: "subscription_plan_prices",
                        principalColumn: "plan_price_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_subscriptions_subscription_plans_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "billing",
                        principalTable: "subscription_plans",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_payments",
                schema: "billing",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_subscription_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_provider_id = table.Column<long>(type: "bigint", nullable: false),
                    payment_status_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    currency_id = table.Column<long>(type: "bigint", nullable: false),
                    provider_payment_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "fk_subscription_payments_master_values_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subscription_payments_master_values_payment_provider_id",
                        column: x => x.payment_provider_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subscription_payments_master_values_payment_status_id",
                        column: x => x.payment_status_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subscription_payments_user_subscriptions_user_subscription_",
                        column: x => x.user_subscription_id,
                        principalSchema: "billing",
                        principalTable: "user_subscriptions",
                        principalColumn: "user_subscription_id");
                });

            migrationBuilder.CreateTable(
                name: "user_subscription_history",
                schema: "billing",
                columns: table => new
                {
                    history_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_plan_id = table.Column<long>(type: "bigint", nullable: true),
                    new_plan_id = table.Column<long>(type: "bigint", nullable: false),
                    old_plan_price_id = table.Column<long>(type: "bigint", nullable: true),
                    new_plan_price_id = table.Column<long>(type: "bigint", nullable: false),
                    change_type_id = table.Column<long>(type: "bigint", nullable: false),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "(NOW() AT TIME ZONE 'UTC')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_subscription_history", x => x.history_id);
                    table.ForeignKey(
                        name: "fk_user_subscription_history_master_values_change_type_id",
                        column: x => x.change_type_id,
                        principalSchema: "master",
                        principalTable: "master_values",
                        principalColumn: "master_value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_subscription_history_user_subscriptions_user_subscript",
                        column: x => x.user_subscription_id,
                        principalSchema: "billing",
                        principalTable: "user_subscriptions",
                        principalColumn: "user_subscription_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_master_values_master_type_code",
                schema: "master",
                table: "master_values",
                columns: new[] { "master_type", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscription_payments_currency_id",
                schema: "billing",
                table: "subscription_payments",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_payments_payment_provider_id",
                schema: "billing",
                table: "subscription_payments",
                column: "payment_provider_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_payments_payment_status_id",
                schema: "billing",
                table: "subscription_payments",
                column: "payment_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_payments_user_subscription_id",
                schema: "billing",
                table: "subscription_payments",
                column: "user_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plan_prices_billing_cycle_id",
                schema: "billing",
                table: "subscription_plan_prices",
                column: "billing_cycle_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plan_prices_currency_id",
                schema: "billing",
                table: "subscription_plan_prices",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plan_prices_plan_id",
                schema: "billing",
                table: "subscription_plan_prices",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plans_plan_code_id",
                schema: "billing",
                table: "subscription_plans",
                column: "plan_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_subscription_history_change_type_id",
                schema: "billing",
                table: "user_subscription_history",
                column: "change_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_subscription_history_user_subscription_id",
                schema: "billing",
                table: "user_subscription_history",
                column: "user_subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_subscriptions_plan_id",
                schema: "billing",
                table: "user_subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_subscriptions_plan_price_id",
                schema: "billing",
                table: "user_subscriptions",
                column: "plan_price_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_subscriptions_status_id",
                schema: "billing",
                table: "user_subscriptions",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_subscriptions_user_id",
                schema: "billing",
                table: "user_subscriptions",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscription_payments",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "user_subscription_history",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "user_subscriptions",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "subscription_plan_prices",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "subscription_plans",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "master_values",
                schema: "master");
        }
    }
}
