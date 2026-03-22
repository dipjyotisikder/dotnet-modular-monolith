using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedAuditableProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TokenRevocationVersion",
                schema: "Users",
                table: "Users",
                type: "text",
                nullable: true,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "{}");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "Users",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "Users",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                schema: "Users",
                table: "Users",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Users",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "TokenRevocationVersion",
                schema: "Users",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldDefaultValue: "{}");
        }
    }
}
