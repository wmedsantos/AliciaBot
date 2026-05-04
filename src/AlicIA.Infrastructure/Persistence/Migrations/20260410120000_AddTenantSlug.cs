using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlicIA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "tenants",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                table: "tenants",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenants_Slug",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "tenants");
        }
    }
}
