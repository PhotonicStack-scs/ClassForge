using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGapItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LanguagePreference",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultLanguage",
                table: "Tenants",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "nb");

            migrationBuilder.AddColumn<bool>(
                name: "SetupCompleted",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SetupProgressJson",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Subjects",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#DBEAFE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultLanguage",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SetupCompleted",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SetupProgressJson",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Subjects");
        }
    }
}
