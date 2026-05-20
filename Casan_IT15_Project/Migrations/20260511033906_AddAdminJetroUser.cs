using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Casan_IT15_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminJetroUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CompanyId", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "Phone", "UpdatedAt", "Username" },
                values: new object[] { 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin_jetro@gmail.com", "Admin", true, null, "Jetro", "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", null, null, "admin_jetro@gmail.com" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserRoleId", "AssignedAt", "RoleId", "UserId" },
                values: new object[] { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);
        }
    }
}
