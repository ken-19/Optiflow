using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Casan_IT15_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToInventoryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "InventoryItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "InventoryItems",
                keyColumn: "InventoryId",
                keyValue: 1,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "InventoryItems",
                keyColumn: "InventoryId",
                keyValue: 2,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "InventoryItems",
                keyColumn: "InventoryId",
                keyValue: 3,
                column: "IsActive",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "InventoryItems");
        }
    }
}
