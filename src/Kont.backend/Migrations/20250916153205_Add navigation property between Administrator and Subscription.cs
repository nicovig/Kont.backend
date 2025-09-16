using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kont.backend.Migrations
{
    /// <inheritdoc />
    public partial class AddnavigationpropertybetweenAdministratorandSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_administrator_subscription_id",
                table: "administrator");

            migrationBuilder.CreateIndex(
                name: "ix_administrator_subscription_id",
                table: "administrator",
                column: "subscription_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_administrator_subscription_id",
                table: "administrator");

            migrationBuilder.CreateIndex(
                name: "ix_administrator_subscription_id",
                table: "administrator",
                column: "subscription_id");
        }
    }
}
