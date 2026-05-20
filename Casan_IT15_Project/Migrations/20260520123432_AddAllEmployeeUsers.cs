using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Casan_IT15_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddAllEmployeeUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CompanyId", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "Phone", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 3, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "planner_jetro@gmail.com", "Planner", true, null, "Jetro", "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", null, null, "planner_jetro@gmail.com" },
                    { 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "inventory_jetro@gmail.com", "Inventory", true, null, "Jetro", "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", null, null, "inventory_jetro@gmail.com" },
                    { 5, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "cost_jetro@gmail.com", "Cost", true, null, "Jetro", "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", null, null, "cost_jetro@gmail.com" },
                    { 6, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "supervisor_jetro@gmail.com", "Supervisor", true, null, "Jetro", "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", null, null, "supervisor_jetro@gmail.com" },
                    { 7, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "quality_jetro@gmail.com", "Quality", true, null, "Jetro", "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", null, null, "quality_jetro@gmail.com" },
                    { 8, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "manager_jetro@gmail.com", "Manager", true, null, "Jetro", "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", null, null, "manager_jetro@gmail.com" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserRoleId", "AssignedAt", "RoleId", "UserId" },
                values: new object[,]
                {
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 3 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 4 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 5 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 6 },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, 7 },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "UserRoleId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 8);
        }
    }
}
