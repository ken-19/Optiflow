using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Casan_IT15_Project.Migrations
{
    /// <inheritdoc />
    public partial class MultiTenantIsolationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CompanyId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_Materials_CompanyId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_MaterialCode",
                table: "Materials");

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "CompanyId", "Address", "CompanyName", "ContactEmail", "ContactPhone", "CreatedAt", "Industry", "IsActive", "SubscriptionExpiry", "SubscriptionPlan", "UpdatedAt" },
                values: new object[] { 2, "456 Production Blvd, Cebu, Philippines", "Jetro Manufacturing", "admin_jetro@gmail.com", "+63-917-123-4567", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manufacturing", true, new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Premium Plan", null });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "RolePermissionId", "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 30, 1, 2 },
                    { 31, 2, 2 },
                    { 32, 3, 2 },
                    { 33, 4, 2 },
                    { 34, 5, 2 },
                    { 35, 6, 2 },
                    { 36, 7, 2 },
                    { 37, 8, 2 },
                    { 38, 9, 2 },
                    { 39, 10, 2 },
                    { 40, 11, 2 },
                    { 41, 12, 2 },
                    { 42, 13, 2 },
                    { 43, 14, 2 },
                    { 44, 15, 2 },
                    { 45, 16, 2 },
                    { 46, 17, 2 },
                    { 47, 18, 2 },
                    { 48, 19, 2 },
                    { 49, 20, 2 },
                    { 50, 21, 2 },
                    { 51, 22, 2 },
                    { 52, 23, 2 },
                    { 53, 24, 2 },
                    { 54, 25, 2 },
                    { 55, 26, 2 },
                    { 56, 27, 2 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CompanyId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CompanyId",
                value: 2);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CompanyId_WorkOrderNumber",
                table: "WorkOrders",
                columns: new[] { "CompanyId", "WorkOrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_CompanyId_MaterialCode",
                table: "Materials",
                columns: new[] { "CompanyId", "MaterialCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CompanyId_WorkOrderNumber",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_Materials_CompanyId_MaterialCode",
                table: "Materials");

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "CompanyId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "RolePermissionId",
                keyValue: 56);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "CompanyId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "CompanyId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CompanyId",
                table: "WorkOrders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders",
                column: "WorkOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_CompanyId",
                table: "Materials",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_MaterialCode",
                table: "Materials",
                column: "MaterialCode",
                unique: true);
        }
    }
}
