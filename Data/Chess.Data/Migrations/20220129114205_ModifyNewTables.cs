using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chess.Data.Migrations
{
    public partial class ModifyNewTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notation",
                table: "moves",
                newName: "notation");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_on",
                table: "moves",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_on",
                table: "moves",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_on",
                table: "moves");

            migrationBuilder.DropColumn(
                name: "modified_on",
                table: "moves");

            migrationBuilder.RenameColumn(
                name: "notation",
                table: "moves",
                newName: "Notation");
        }
    }
}
