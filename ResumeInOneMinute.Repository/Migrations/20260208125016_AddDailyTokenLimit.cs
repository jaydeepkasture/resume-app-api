using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResumeInOneMinute.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyTokenLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 1L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1362));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 2L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1365));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 3L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1366));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 4L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1367));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 5L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1368));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 6L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1369));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 7L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1370));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 8L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1371));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 9L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1372));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 10L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1374));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 11L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1375));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 12L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1376));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 13L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1377));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 14L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1378));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 15L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 16L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1381));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 17L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1382));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 18L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1383));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 19L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1385));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 1L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1559));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 2L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1561));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 3L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1563));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 4L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1564));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefits",
                keyColumn: "benefit_id",
                keyValue: 1L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1529));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefits",
                keyColumn: "benefit_id",
                keyValue: 2L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1531));

            migrationBuilder.InsertData(
                schema: "billing",
                table: "plan_benefits",
                columns: new[] { "benefit_id", "benefit_code", "benefit_name", "created_at", "description", "is_active" },
                values: new object[] { 3L, "DAILY_TOKEN_LIMIT", "Daily Token Limit", new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1532), "Total instruction message characters per day", true });

            migrationBuilder.InsertData(
                schema: "billing",
                table: "plan_benefit_map",
                columns: new[] { "map_id", "benefit_id", "benefit_value", "created_at", "plan_id" },
                values: new object[,]
                {
                    { 5L, 3L, "3000", new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1562), 1L },
                    { 6L, 3L, "10000", new DateTime(2026, 2, 8, 12, 50, 15, 716, DateTimeKind.Utc).AddTicks(1565), 2L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                schema: "billing",
                table: "plan_benefits",
                keyColumn: "benefit_id",
                keyValue: 3L);

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 1L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6572));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 2L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6579));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 3L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6580));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 4L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6581));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 5L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6582));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 6L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6584));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 7L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6585));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 8L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6586));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 9L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6587));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 10L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6588));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 11L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6589));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 12L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6591));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 13L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6592));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 14L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6593));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 15L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6594));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 16L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6595));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 17L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6596));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 18L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6597));

            migrationBuilder.UpdateData(
                schema: "master",
                table: "master_values",
                keyColumn: "master_value_id",
                keyValue: 19L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6599));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 1L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6769));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 2L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6771));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 3L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6772));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefit_map",
                keyColumn: "map_id",
                keyValue: 4L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6773));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefits",
                keyColumn: "benefit_id",
                keyValue: 1L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6737));

            migrationBuilder.UpdateData(
                schema: "billing",
                table: "plan_benefits",
                keyColumn: "benefit_id",
                keyValue: 2L,
                column: "created_at",
                value: new DateTime(2026, 2, 8, 12, 22, 35, 797, DateTimeKind.Utc).AddTicks(6739));
        }
    }
}
