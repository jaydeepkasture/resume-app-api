using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResumeInOneMinute.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanBenefits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS billing.plan_benefit_map CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS billing.plan_benefits CASCADE;");

            migrationBuilder.AddColumn<long>(
                name: "plan_price_id",
                schema: "billing",
                table: "subscription_payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "plan_benefits",
                schema: "billing",
                columns: table => new
                {
                    benefit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    benefit_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    benefit_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_benefits", x => x.benefit_id);
                });

            migrationBuilder.CreateTable(
                name: "plan_benefit_map",
                schema: "billing",
                columns: table => new
                {
                    map_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plan_id = table.Column<long>(type: "bigint", nullable: false),
                    benefit_id = table.Column<long>(type: "bigint", nullable: false),
                    benefit_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_benefit_map", x => x.map_id);
                    table.ForeignKey(
                        name: "fk_plan_benefit_map_plan_benefits_benefit_id",
                        column: x => x.benefit_id,
                        principalSchema: "billing",
                        principalTable: "plan_benefits",
                        principalColumn: "benefit_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plan_benefit_map_subscription_plans_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "billing",
                        principalTable: "subscription_plans",
                        principalColumn: "plan_id",
                        onDelete: ReferentialAction.Cascade);
                });


            migrationBuilder.InsertData(
                schema: "billing",
                table: "plan_benefits",
                columns: new[] { "benefit_id", "benefit_code", "benefit_name", "created_at", "description", "is_active" },
                values: new object[,]
                {
                    { 1L, "TEMPLATE_LIMIT", "Template Limit", new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6737), "Number of templates allowed", true },
                    { 2L, "RATE_LIMIT_PER_MINUTE", "Rate Limit", new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6739), "API requests per minute", true }
                });

            migrationBuilder.InsertData(
                schema: "billing",
                table: "plan_benefit_map",
                columns: new[] { "map_id", "benefit_id", "benefit_value", "created_at", "plan_id" },
                values: new object[,]
                {
                    { 1L, 1L, "3", new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6769), 1L },
                    { 2L, 2L, "30", new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6771), 1L },
                    { 3L, 1L, "50", new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6772), 2L },
                    { 4L, 2L, "300", new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6773), 2L }
                });

            migrationBuilder.CreateIndex(
                name: "ix_subscription_payments_plan_price_id",
                schema: "billing",
                table: "subscription_payments",
                column: "plan_price_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_benefit_map_benefit_id",
                schema: "billing",
                table: "plan_benefit_map",
                column: "benefit_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_benefit_map_plan_id",
                schema: "billing",
                table: "plan_benefit_map",
                column: "plan_id");

            migrationBuilder.AddForeignKey(
                name: "fk_subscription_payments_subscription_plan_prices_plan_price_id",
                schema: "billing",
                table: "subscription_payments",
                column: "plan_price_id",
                principalSchema: "billing",
                principalTable: "subscription_plan_prices",
                principalColumn: "plan_price_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_subscription_payments_subscription_plan_prices_plan_price_id",
                schema: "billing",
                table: "subscription_payments");

            migrationBuilder.DropTable(
                name: "plan_benefit_map",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "plan_benefits",
                schema: "billing");

            migrationBuilder.DropIndex(
                name: "ix_subscription_payments_plan_price_id",
                schema: "billing",
                table: "subscription_payments");

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 19L);

            migrationBuilder.DropColumn(
                name: "plan_price_id",
                schema: "billing",
                table: "subscription_payments");
        }
    }
}
