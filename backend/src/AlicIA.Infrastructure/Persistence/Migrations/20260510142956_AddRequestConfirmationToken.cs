using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlicIA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestConfirmationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationTokenHash",
                table: "requests",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_requests_ConfirmationTokenHash",
                table: "requests",
                column: "ConfirmationTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_requests_ConfirmationTokenHash",
                table: "requests");

            migrationBuilder.DropColumn(
                name: "ConfirmationTokenHash",
                table: "requests");
        }
    }
}
