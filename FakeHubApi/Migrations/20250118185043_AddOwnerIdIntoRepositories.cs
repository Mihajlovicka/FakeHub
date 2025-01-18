using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeHubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdIntoRepositories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Repositories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Repositories");
        }
    }
}
