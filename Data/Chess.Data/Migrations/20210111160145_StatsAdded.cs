using Microsoft.EntityFrameworkCore.Migrations;

namespace Chess.Data.Migrations
{
    public partial class StatsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDraws",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "TotalGames",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "TotalLosses",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "TotalWons",
                table: "Stats");

            migrationBuilder.AddColumn<int>(
                name: "Draws",
                table: "Stats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Losses",
                table: "Stats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Matches",
                table: "Stats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wons",
                table: "Stats",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Draws",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Losses",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Matches",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Wons",
                table: "Stats");

            migrationBuilder.AddColumn<int>(
                name: "TotalDraws",
                table: "Stats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalGames",
                table: "Stats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalLosses",
                table: "Stats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalWons",
                table: "Stats",
                type: "int",
                nullable: true);
        }
    }
}
