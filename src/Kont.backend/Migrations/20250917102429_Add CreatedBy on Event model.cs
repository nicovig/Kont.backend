using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kont.backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByonEventmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "created_by_id",
                table: "event",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_event_created_by_id",
                table: "event",
                column: "created_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_event_administrator_created_by_id",
                table: "event",
                column: "created_by_id",
                principalTable: "administrator",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_event_administrator_created_by_id",
                table: "event");

            migrationBuilder.DropIndex(
                name: "ix_event_created_by_id",
                table: "event");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "event");
        }
    }
}
