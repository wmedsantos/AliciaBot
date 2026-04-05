using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlicIA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendar_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CalendarEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CalendarId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_connections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_calendar_connections_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calendar_connections_TenantId",
                table: "calendar_connections",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar_connections");
        }
    }
}
