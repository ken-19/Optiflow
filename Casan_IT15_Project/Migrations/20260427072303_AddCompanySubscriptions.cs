using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Casan_IT15_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanySubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiry",
                table: "Companies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionPlan",
                table: "Companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "CompanyId",
                keyValue: 1,
                columns: new[] { "SubscriptionExpiry", "SubscriptionPlan" },
                values: new object[] { null, "None" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionExpiry",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlan",
                table: "Companies");
        }
    }
}
