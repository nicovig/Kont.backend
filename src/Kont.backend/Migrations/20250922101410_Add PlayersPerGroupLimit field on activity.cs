using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kont.backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayersPerGroupLimitfieldonactivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "player_type",
                table: "player");

            migrationBuilder.AddColumn<string>(
                name: "player_type",
                table: "player_registration",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "players_per_group_limit",
                table: "activity",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "player_type",
                table: "player_registration");

            migrationBuilder.DropColumn(
                name: "players_per_group_limit",
                table: "activity");

            migrationBuilder.AddColumn<string>(
                name: "player_type",
                table: "player",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
