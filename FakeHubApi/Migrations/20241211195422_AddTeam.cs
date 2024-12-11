using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace FakeHubApi.Migrations
{
    public partial class AddTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Team table
            migrationBuilder
                .CreateTable(
                    name: "Team",
                    columns: table => new
                    {
                        Id = table
                            .Column<int>(type: "int", nullable: false)
                            .Annotation(
                                "MySQL:ValueGenerationStrategy",
                                MySQLValueGenerationStrategy.IdentityColumn
                            ),
                        Name = table.Column<string>(type: "longtext", nullable: false),
                        Description = table.Column<string>(type: "longtext", nullable: false),
                        CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                        OrganizationId = table.Column<int>(type: "int", nullable: false),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Team", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Team_Organizations_OrganizationId",
                            column: x => x.OrganizationId,
                            principalTable: "Organizations",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            // Create index for the OrganizationId in Team
            migrationBuilder.CreateIndex(
                name: "IX_Team_OrganizationId",
                table: "Team",
                column: "OrganizationId"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Team table
            migrationBuilder.DropTable(name: "Team");
        }
    }
}
