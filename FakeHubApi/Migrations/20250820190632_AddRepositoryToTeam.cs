using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeHubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRepositoryToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepositoryId",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_RepositoryId",
                table: "Teams",
                column: "RepositoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Repositories_RepositoryId",
                table: "Teams",
                column: "RepositoryId",
                principalTable: "Repositories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Repositories_RepositoryId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_RepositoryId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "Teams");
        }
    }
}
