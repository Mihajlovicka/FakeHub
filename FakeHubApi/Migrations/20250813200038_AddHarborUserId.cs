using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeHubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHarborUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HarborUserId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HarborUserId",
                table: "AspNetUsers");
        }
    }
}
