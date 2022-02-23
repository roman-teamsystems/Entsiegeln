using Microsoft.EntityFrameworkCore.Migrations;

namespace Entsiegeln.Migrations
{
    public partial class Viable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Viable",
                table: "Projekte",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Viable",
                table: "Projekte");
        }
    }
}
