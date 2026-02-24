using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveConstraintsToGradeSubjectRequirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowDoublePeriods",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "MaxPeriodsPerDay",
                table: "Subjects");

            migrationBuilder.AddColumn<bool>(
                name: "AllowDoublePeriods",
                table: "GradeSubjectRequirements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxPeriodsPerDay",
                table: "GradeSubjectRequirements",
                type: "integer",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowDoublePeriods",
                table: "GradeSubjectRequirements");

            migrationBuilder.DropColumn(
                name: "MaxPeriodsPerDay",
                table: "GradeSubjectRequirements");

            migrationBuilder.AddColumn<bool>(
                name: "AllowDoublePeriods",
                table: "Subjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxPeriodsPerDay",
                table: "Subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
