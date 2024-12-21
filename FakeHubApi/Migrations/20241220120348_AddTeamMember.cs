using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeHubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateTable(
                    name: "UserTeam",
                    columns: table => new
                    {
                        TeamId = table.Column<int>(type: "int", nullable: false),
                        UserId = table.Column<int>(type: "int", nullable: false),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_UserTeam", x => new { x.TeamId, x.UserId });
                        table.ForeignKey(
                            name: "FK_UserTeam_AspNetUsers_UserId",
                            column: x => x.UserId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                        table.ForeignKey(
                            name: "FK_UserTeam_Teams_TeamId",
                            column: x => x.TeamId,
                            principalTable: "Teams",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserTeam_UserId",
                table: "UserTeam",
                column: "UserId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserTeam");
        }
    }
}
