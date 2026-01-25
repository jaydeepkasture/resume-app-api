using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeInOneMinute.Repository.Migrations
{
    /// <inheritdoc />
    public partial class StoreRefreshTokenAsHashSalt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DropColumn(
                name: "refresh_token",
                schema: "auth",
                table: "users");
            */

            /*
            migrationBuilder.AddColumn<byte[]>(
                name: "refresh_token_hash",
                schema: "auth",
                table: "users",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "refresh_token_salt",
                schema: "auth",
                table: "users",
                type: "bytea",
                nullable: true);
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refresh_token_hash",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "refresh_token_salt",
                schema: "auth",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "refresh_token",
                schema: "auth",
                table: "users",
                type: "text",
                nullable: true);
        }
    }
}
