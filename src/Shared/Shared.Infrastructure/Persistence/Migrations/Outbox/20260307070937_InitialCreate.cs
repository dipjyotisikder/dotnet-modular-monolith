using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.Outbox
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Shared");

            migrationBuilder.CreateTable(
                name: "Outbox",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    EventVersion = table.Column<int>(type: "integer", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sent = table.Column<bool>(type: "boolean", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    IsDeadLettered = table.Column<bool>(type: "boolean", nullable: false),
                    DeadLetteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_CorrelationId",
                schema: "Shared",
                table: "Outbox",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_CreatedAt",
                schema: "Shared",
                table: "Outbox",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_DeadLetteredAt",
                schema: "Shared",
                table: "Outbox",
                column: "DeadLetteredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_IdempotencyKey",
                schema: "Shared",
                table: "Outbox",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_IsDeadLettered_Sent_RetryCount",
                schema: "Shared",
                table: "Outbox",
                columns: new[] { "IsDeadLettered", "Sent", "RetryCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Outbox",
                schema: "Shared");
        }
    }
}
