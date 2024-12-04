using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace FakeHubApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateTable(
                    name: "Organizations",
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
                        ImageBase64 = table.Column<string>(type: "longtext", nullable: false),
                        IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                        OwnerId = table.Column<int>(type: "int", nullable: false),
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Organizations", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Organizations_AspNetUsers_OwnerId",
                            column: x => x.OwnerId,
                            principalTable: "AspNetUsers",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade
                        );
                    }
                )
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OwnerId",
                table: "Organizations",
                column: "OwnerId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Organizations");
        }
    }
}
