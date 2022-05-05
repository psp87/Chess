using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chess.Data.Migrations
{
    public partial class ModifyGamesAndMovesAndErrorLogsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stats_user_id",
                table: "stats");

            migrationBuilder.RenameColumn(
                name: "win",
                table: "stats",
                newName: "won");

            migrationBuilder.RenameColumn(
                name: "loss",
                table: "stats",
                newName: "played");

            migrationBuilder.RenameColumn(
                name: "games",
                table: "stats",
                newName: "lost");

            migrationBuilder.RenameColumn(
                name: "draw",
                table: "stats",
                newName: "drawn");

            migrationBuilder.RenameColumn(
                name: "player_2",
                table: "games",
                newName: "player_two_user_id");

            migrationBuilder.RenameColumn(
                name: "player_1",
                table: "games",
                newName: "player_two_name");

            migrationBuilder.AlterColumn<string>(
                name: "notation",
                table: "moves",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "moves",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "player_one_name",
                table: "games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "player_one_user_id",
                table: "games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "error_logs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_stats_user_id",
                table: "stats",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_moves_user_id",
                table: "moves",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_moves_users_user_id",
                table: "moves",
                column: "user_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_moves_users_user_id",
                table: "moves");

            migrationBuilder.DropIndex(
                name: "IX_stats_user_id",
                table: "stats");

            migrationBuilder.DropIndex(
                name: "IX_moves_user_id",
                table: "moves");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "moves");

            migrationBuilder.DropColumn(
                name: "player_one_name",
                table: "games");

            migrationBuilder.DropColumn(
                name: "player_one_user_id",
                table: "games");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "error_logs");

            migrationBuilder.RenameColumn(
                name: "won",
                table: "stats",
                newName: "win");

            migrationBuilder.RenameColumn(
                name: "played",
                table: "stats",
                newName: "loss");

            migrationBuilder.RenameColumn(
                name: "lost",
                table: "stats",
                newName: "games");

            migrationBuilder.RenameColumn(
                name: "drawn",
                table: "stats",
                newName: "draw");

            migrationBuilder.RenameColumn(
                name: "player_two_user_id",
                table: "games",
                newName: "player_2");

            migrationBuilder.RenameColumn(
                name: "player_two_name",
                table: "games",
                newName: "player_1");

            migrationBuilder.AlterColumn<string>(
                name: "notation",
                table: "moves",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_stats_user_id",
                table: "stats",
                column: "user_id",
                unique: true);
        }
    }
}
