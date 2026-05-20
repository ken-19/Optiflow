using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Casan_IT15_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$rZJCkpJQ6YBGxQRvpL6Wa.2dHt8sOXEzQqKGJkLKjS5c5B0Wm0FHe");
        }
    }
}
