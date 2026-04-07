using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlicIA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalEventIdToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalEventId",
                table: "requests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalEventId",
                table: "requests");
        }
    }
}
