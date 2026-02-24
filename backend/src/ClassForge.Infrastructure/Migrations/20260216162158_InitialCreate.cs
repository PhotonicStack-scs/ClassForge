using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grades_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teachers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingDays_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExternalProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Groups_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequiresSpecialRoom = table.Column<bool>(type: "boolean", nullable: false),
                    SpecialRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaxPeriodsPerDay = table.Column<int>(type: "integer", nullable: false),
                    AllowDoublePeriods = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Rooms_SpecialRoomId",
                        column: x => x.SpecialRoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Subjects_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradeDayConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxPeriods = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeDayConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeDayConfigs_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GradeDayConfigs_TeachingDays_TeachingDayId",
                        column: x => x.TeachingDayId,
                        principalTable: "TeachingDays",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GradeDayConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherDayConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxPeriods = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherDayConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherDayConfigs_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherDayConfigs_TeachingDays_TeachingDayId",
                        column: x => x.TeachingDayId,
                        principalTable: "TeachingDays",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotNumber = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsBreak = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlots_TeachingDays_TeachingDayId",
                        column: x => x.TeachingDayId,
                        principalTable: "TeachingDays",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimeSlots_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CombinedLessonConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    MaxGroupsPerLesson = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombinedLessonConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombinedLessonConfigs_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CombinedLessonConfigs_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CombinedLessonConfigs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradeSubjectRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodsPerWeek = table.Column<int>(type: "integer", nullable: false),
                    PreferDoublePeriods = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeSubjectRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeSubjectRequirements_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GradeSubjectRequirements_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GradeSubjectRequirements_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSubjectQualifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxGradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSubjectQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectQualifications_Grades_MaxGradeId",
                        column: x => x.MaxGradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherSubjectQualifications_Grades_MinGradeId",
                        column: x => x.MinGradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherSubjectQualifications_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherSubjectQualifications_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSlotBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSlotBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSlotBlocks_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherSlotBlocks_TimeSlots_TimeSlotId",
                        column: x => x.TimeSlotId,
                        principalTable: "TimeSlots",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CombinedLessonGroups",
                columns: table => new
                {
                    CombinedLessonConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombinedLessonGroups", x => new { x.CombinedLessonConfigId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_CombinedLessonGroups_CombinedLessonConfigs_CombinedLessonCo~",
                        column: x => x.CombinedLessonConfigId,
                        principalTable: "CombinedLessonConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CombinedLessonGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CombinedLessonConfigs_GradeId",
                table: "CombinedLessonConfigs",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_CombinedLessonConfigs_SubjectId",
                table: "CombinedLessonConfigs",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CombinedLessonConfigs_TenantId",
                table: "CombinedLessonConfigs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CombinedLessonGroups_GroupId",
                table: "CombinedLessonGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeDayConfigs_GradeId",
                table: "GradeDayConfigs",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeDayConfigs_TeachingDayId",
                table: "GradeDayConfigs",
                column: "TeachingDayId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeDayConfigs_TenantId_GradeId_TeachingDayId",
                table: "GradeDayConfigs",
                columns: new[] { "TenantId", "GradeId", "TeachingDayId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_TenantId_Name",
                table: "Grades",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjectRequirements_GradeId",
                table: "GradeSubjectRequirements",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjectRequirements_SubjectId",
                table: "GradeSubjectRequirements",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjectRequirements_TenantId_GradeId_SubjectId",
                table: "GradeSubjectRequirements",
                columns: new[] { "TenantId", "GradeId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_GradeId",
                table: "Groups",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TenantId_GradeId_Name",
                table: "Groups",
                columns: new[] { "TenantId", "GradeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_TenantId_Name",
                table: "Rooms",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SpecialRoomId",
                table: "Subjects",
                column: "SpecialRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_TenantId_Name",
                table: "Subjects",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherDayConfigs_TeacherId_TeachingDayId",
                table: "TeacherDayConfigs",
                columns: new[] { "TeacherId", "TeachingDayId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherDayConfigs_TeachingDayId",
                table: "TeacherDayConfigs",
                column: "TeachingDayId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_TenantId",
                table: "Teachers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSlotBlocks_TeacherId_TimeSlotId",
                table: "TeacherSlotBlocks",
                columns: new[] { "TeacherId", "TimeSlotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSlotBlocks_TimeSlotId",
                table: "TeacherSlotBlocks",
                column: "TimeSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectQualifications_MaxGradeId",
                table: "TeacherSubjectQualifications",
                column: "MaxGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectQualifications_MinGradeId",
                table: "TeacherSubjectQualifications",
                column: "MinGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectQualifications_SubjectId",
                table: "TeacherSubjectQualifications",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectQualifications_TeacherId_SubjectId",
                table: "TeacherSubjectQualifications",
                columns: new[] { "TeacherId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingDays_TenantId_DayOfWeek",
                table: "TeachingDays",
                columns: new[] { "TenantId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_TeachingDayId_SlotNumber",
                table: "TimeSlots",
                columns: new[] { "TeachingDayId", "SlotNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_TenantId",
                table: "TimeSlots",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CombinedLessonGroups");

            migrationBuilder.DropTable(
                name: "GradeDayConfigs");

            migrationBuilder.DropTable(
                name: "GradeSubjectRequirements");

            migrationBuilder.DropTable(
                name: "TeacherDayConfigs");

            migrationBuilder.DropTable(
                name: "TeacherSlotBlocks");

            migrationBuilder.DropTable(
                name: "TeacherSubjectQualifications");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CombinedLessonConfigs");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "TeachingDays");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
