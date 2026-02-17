using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetableEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Timetables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualityScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timetables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timetables_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Timetables_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TimetableEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimetableId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDoublePeriod = table.Column<bool>(type: "boolean", nullable: false),
                    CombinedLessonGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntries_TimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "TimeSlots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntries_Timetables_TimetableId",
                        column: x => x.TimetableId,
                        principalTable: "Timetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimetableId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableReports_Timetables_TimetableId",
                        column: x => x.TimetableId,
                        principalTable: "Timetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableEntryGroups",
                columns: table => new
                {
                    TimetableEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableEntryGroups", x => new { x.TimetableEntryId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_TimetableEntryGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntryGroups_TimetableEntries_TimetableEntryId",
                        column: x => x.TimetableEntryId,
                        principalTable: "TimetableEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_RoomId",
                table: "TimetableEntries",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_SubjectId",
                table: "TimetableEntries",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_TeacherId",
                table: "TimetableEntries",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_TimeSlotId",
                table: "TimetableEntries",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntries_TimetableId_TimeSlotId",
                table: "TimetableEntries",
                columns: new[] { "TimetableId", "TimeSlotId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntryGroups_GroupId",
                table: "TimetableEntryGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableReports_TimetableId",
                table: "TimetableReports",
                column: "TimetableId");

            migrationBuilder.CreateIndex(
                name: "IX_Timetables_CreatedBy",
                table: "Timetables",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Timetables_TenantId",
                table: "Timetables",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimetableEntryGroups");

            migrationBuilder.DropTable(
                name: "TimetableReports");

            migrationBuilder.DropTable(
                name: "TimetableEntries");

            migrationBuilder.DropTable(
                name: "Timetables");
        }
    }
}
