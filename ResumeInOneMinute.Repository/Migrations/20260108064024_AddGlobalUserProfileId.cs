using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResumeInOneMinute.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalUserProfileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "global_user_profile_id",
                schema: "auth",
                table: "user_profiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "global_user_profile_id",
                schema: "auth",
                table: "user_profiles");
        }
    }
}
