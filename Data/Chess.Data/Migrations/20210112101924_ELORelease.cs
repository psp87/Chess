using Microsoft.EntityFrameworkCore.Migrations;

namespace Chess.Data.Migrations
{
    public partial class ELORelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stats_AspNetUsers_ApplicationUserId",
                table: "Stats");

            migrationBuilder.DropIndex(
                name: "IX_Stats_ApplicationUserId",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Matches",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Wons",
                table: "Stats");

            migrationBuilder.AddColumn<int>(
                name: "Games",
                table: "Stats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Stats",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Stats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wins",
                table: "Stats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Stats_OwnerId",
                table: "Stats",
                column: "OwnerId",
                unique: true,
                filter: "[OwnerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Stats_AspNetUsers_OwnerId",
                table: "Stats",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stats_AspNetUsers_OwnerId",
                table: "Stats");

            migrationBuilder.DropIndex(
                name: "IX_Stats_OwnerId",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Games",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Wins",
                table: "Stats");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Stats",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Matches",
                table: "Stats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wons",
                table: "Stats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Stats_ApplicationUserId",
                table: "Stats",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Stats_AspNetUsers_ApplicationUserId",
                table: "Stats",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
