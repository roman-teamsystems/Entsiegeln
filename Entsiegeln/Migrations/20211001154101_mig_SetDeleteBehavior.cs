using Microsoft.EntityFrameworkCore.Migrations;

namespace Entsiegeln.Migrations
{
    public partial class mig_SetDeleteBehavior : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projekte_AspNetUsers_UserId",
                table: "Projekte");

            migrationBuilder.AddForeignKey(
                name: "FK_Projekte_AspNetUsers_UserId",
                table: "Projekte",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projekte_AspNetUsers_UserId",
                table: "Projekte");

            migrationBuilder.AddForeignKey(
                name: "FK_Projekte_AspNetUsers_UserId",
                table: "Projekte",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
