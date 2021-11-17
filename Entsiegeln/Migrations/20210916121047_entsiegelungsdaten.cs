using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace Entsiegeln.Migrations
{
    public partial class entsiegelungsdaten : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Errors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorCode = table.Column<int>(type: "int", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    User = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Errors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icon1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon5 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projekte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bezeichnung = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Beitragender = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Koordinaten = table.Column<Geometry>(type: "geometry", nullable: true),
                    Strasse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Plz = table.Column<string>(type: "VARCHAR(5)", maxLength: 5, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BSV = table.Column<bool>(type: "bit", nullable: false),
                    Kub = table.Column<bool>(type: "bit", nullable: false),
                    Bpf = table.Column<bool>(type: "bit", nullable: false),
                    PzuB = table.Column<bool>(type: "bit", nullable: false),
                    PentsV = table.Column<bool>(type: "bit", nullable: false),
                    VzuG = table.Column<bool>(type: "bit", nullable: false),
                    Div = table.Column<bool>(type: "bit", nullable: false),
                    Vbeet = table.Column<bool>(type: "bit", nullable: false),
                    PP = table.Column<bool>(type: "bit", nullable: false),
                    UG = table.Column<bool>(type: "bit", nullable: false),
                    AzuX = table.Column<bool>(type: "bit", nullable: false),
                    GwPI = table.Column<bool>(type: "bit", nullable: false),
                    RuF = table.Column<bool>(type: "bit", nullable: false),
                    Prio = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projekte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bilder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bilder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bilder_Projekte_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Like = table.Column<bool>(type: "bit", nullable: true),
                    Favorite = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ratings_Projekte_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bilder_ProjectId",
                table: "Bilder",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ProjectId",
                table: "Ratings",
                column: "ProjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bilder");

            migrationBuilder.DropTable(
                name: "Errors");

            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Projekte");
        }
    }
}
