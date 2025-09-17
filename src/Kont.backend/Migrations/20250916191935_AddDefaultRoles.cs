using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kont.backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "role_type", "created_at" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "God", DateTime.UtcNow },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Admin", DateTime.UtcNow },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Manager", DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValues: new object[]
                {
                    new Guid("11111111-1111-1111-1111-111111111111"),
                    new Guid("22222222-2222-2222-2222-222222222222"),
                    new Guid("33333333-3333-3333-3333-333333333333")
                });
        }
    }
}
