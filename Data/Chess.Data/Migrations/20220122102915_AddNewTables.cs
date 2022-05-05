using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chess.Data.Migrations
{
    public partial class AddNewTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    player_1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    player_2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_on = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "error_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    created_on = table.Column<DateTime>(type: "datetime2", nullable: false),
                    source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fen_string = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    exception_message = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_error_logs_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "moves",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Notation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    game_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moves", x => x.id);
                    table.ForeignKey(
                        name: "FK_moves_games_game_id",
                        column: x => x.game_id,
                        principalTable: "games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_game_id",
                table: "error_logs",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_moves_game_id",
                table: "moves",
                column: "game_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "error_logs");

            migrationBuilder.DropTable(
                name: "moves");

            migrationBuilder.DropTable(
                name: "games");
        }
    }
}
