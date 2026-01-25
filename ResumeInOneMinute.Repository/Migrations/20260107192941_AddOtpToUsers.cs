using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeInOneMinute.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "otp_code",
                schema: "auth",
                table: "users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "otp_expiry_time",
                schema: "auth",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "otp_code",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "otp_expiry_time",
                schema: "auth",
                table: "users");
        }
    }
}
