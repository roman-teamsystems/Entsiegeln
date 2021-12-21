using Microsoft.EntityFrameworkCore.Migrations;

namespace Entsiegeln.Migrations
{
    public partial class sixthIcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon6",
                table: "Preferences",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon6",
                table: "Preferences");
        }
    }
}
