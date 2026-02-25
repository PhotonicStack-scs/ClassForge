using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTerminology : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FKs from non-renamed tables referencing tables/columns being renamed
            migrationBuilder.DropForeignKey(
                name: "FK_CombinedLessonConfigs_Grades_GradeId",
                table: "CombinedLessonConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherDayConfigs_TeachingDays_TeachingDayId",
                table: "TeacherDayConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSubjectQualifications_Grades_MaxGradeId",
                table: "TeacherSubjectQualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSubjectQualifications_Grades_MinGradeId",
                table: "TeacherSubjectQualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_TeachingDays_TeachingDayId",
                table: "TimeSlots");

            // Rename tables (preserves all data)
            migrationBuilder.RenameTable(
                name: "Grades",
                newName: "Years");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Classes");

            migrationBuilder.RenameTable(
                name: "TeachingDays",
                newName: "SchoolDays");

            migrationBuilder.RenameTable(
                name: "GradeSubjectRequirements",
                newName: "YearCurricula");

            migrationBuilder.RenameTable(
                name: "GradeDayConfigs",
                newName: "YearDayConfigs");

            migrationBuilder.RenameTable(
                name: "CombinedLessonGroups",
                newName: "CombinedLessonClasses");

            migrationBuilder.RenameTable(
                name: "TimetableEntryGroups",
                newName: "TimetableEntryClasses");

            // Rename columns in TimetableEntries
            migrationBuilder.RenameColumn(
                name: "CombinedLessonGroupId",
                table: "TimetableEntries",
                newName: "CombinedLessonClassId");

            // Rename columns in TimeSlots
            migrationBuilder.RenameColumn(
                name: "TeachingDayId",
                table: "TimeSlots",
                newName: "SchoolDayId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeSlots_TeachingDayId_SlotNumber",
                table: "TimeSlots",
                newName: "IX_TimeSlots_SchoolDayId_SlotNumber");

            // Rename columns in TeacherSubjectQualifications
            migrationBuilder.RenameColumn(
                name: "MinGradeId",
                table: "TeacherSubjectQualifications",
                newName: "MinYearId");

            migrationBuilder.RenameColumn(
                name: "MaxGradeId",
                table: "TeacherSubjectQualifications",
                newName: "MaxYearId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherSubjectQualifications_MinGradeId",
                table: "TeacherSubjectQualifications",
                newName: "IX_TeacherSubjectQualifications_MinYearId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherSubjectQualifications_MaxGradeId",
                table: "TeacherSubjectQualifications",
                newName: "IX_TeacherSubjectQualifications_MaxYearId");

            // Rename columns in TeacherDayConfigs
            migrationBuilder.RenameColumn(
                name: "TeachingDayId",
                table: "TeacherDayConfigs",
                newName: "SchoolDayId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherDayConfigs_TeachingDayId",
                table: "TeacherDayConfigs",
                newName: "IX_TeacherDayConfigs_SchoolDayId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherDayConfigs_TeacherId_TeachingDayId",
                table: "TeacherDayConfigs",
                newName: "IX_TeacherDayConfigs_TeacherId_SchoolDayId");

            // Rename columns in CombinedLessonConfigs
            migrationBuilder.RenameColumn(
                name: "MaxGroupsPerLesson",
                table: "CombinedLessonConfigs",
                newName: "MaxClassesPerLesson");

            migrationBuilder.RenameColumn(
                name: "GradeId",
                table: "CombinedLessonConfigs",
                newName: "YearId");

            migrationBuilder.RenameIndex(
                name: "IX_CombinedLessonConfigs_GradeId",
                table: "CombinedLessonConfigs",
                newName: "IX_CombinedLessonConfigs_YearId");

            // Rename index on Years (was Grades)
            migrationBuilder.RenameIndex(
                name: "IX_Grades_TenantId_Name",
                table: "Years",
                newName: "IX_Years_TenantId_Name");

            // Rename columns and indexes in Classes (was Groups)
            migrationBuilder.RenameColumn(
                name: "GradeId",
                table: "Classes",
                newName: "YearId");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_GradeId",
                table: "Classes",
                newName: "IX_Classes_YearId");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_TenantId_GradeId_Name",
                table: "Classes",
                newName: "IX_Classes_TenantId_YearId_Name");

            // Rename index on SchoolDays (was TeachingDays)
            migrationBuilder.RenameIndex(
                name: "IX_TeachingDays_TenantId_DayOfWeek",
                table: "SchoolDays",
                newName: "IX_SchoolDays_TenantId_DayOfWeek");

            // Rename columns and indexes in YearCurricula (was GradeSubjectRequirements)
            migrationBuilder.RenameColumn(
                name: "GradeId",
                table: "YearCurricula",
                newName: "YearId");

            migrationBuilder.RenameIndex(
                name: "IX_GradeSubjectRequirements_GradeId",
                table: "YearCurricula",
                newName: "IX_YearCurricula_YearId");

            migrationBuilder.RenameIndex(
                name: "IX_GradeSubjectRequirements_SubjectId",
                table: "YearCurricula",
                newName: "IX_YearCurricula_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_GradeSubjectRequirements_TenantId_GradeId_SubjectId",
                table: "YearCurricula",
                newName: "IX_YearCurricula_TenantId_YearId_SubjectId");

            // Rename columns and indexes in YearDayConfigs (was GradeDayConfigs)
            migrationBuilder.RenameColumn(
                name: "GradeId",
                table: "YearDayConfigs",
                newName: "YearId");

            migrationBuilder.RenameColumn(
                name: "TeachingDayId",
                table: "YearDayConfigs",
                newName: "SchoolDayId");

            migrationBuilder.RenameIndex(
                name: "IX_GradeDayConfigs_GradeId",
                table: "YearDayConfigs",
                newName: "IX_YearDayConfigs_YearId");

            migrationBuilder.RenameIndex(
                name: "IX_GradeDayConfigs_TeachingDayId",
                table: "YearDayConfigs",
                newName: "IX_YearDayConfigs_SchoolDayId");

            migrationBuilder.RenameIndex(
                name: "IX_GradeDayConfigs_TenantId_GradeId_TeachingDayId",
                table: "YearDayConfigs",
                newName: "IX_YearDayConfigs_TenantId_YearId_SchoolDayId");

            // Rename columns and indexes in CombinedLessonClasses (was CombinedLessonGroups)
            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "CombinedLessonClasses",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_CombinedLessonGroups_GroupId",
                table: "CombinedLessonClasses",
                newName: "IX_CombinedLessonClasses_ClassId");

            // Rename columns and indexes in TimetableEntryClasses (was TimetableEntryGroups)
            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "TimetableEntryClasses",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_TimetableEntryGroups_GroupId",
                table: "TimetableEntryClasses",
                newName: "IX_TimetableEntryClasses_ClassId");

            // Re-add FKs in non-renamed tables with new names
            migrationBuilder.AddForeignKey(
                name: "FK_CombinedLessonConfigs_Years_YearId",
                table: "CombinedLessonConfigs",
                column: "YearId",
                principalTable: "Years",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherDayConfigs_SchoolDays_SchoolDayId",
                table: "TeacherDayConfigs",
                column: "SchoolDayId",
                principalTable: "SchoolDays",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSubjectQualifications_Years_MaxYearId",
                table: "TeacherSubjectQualifications",
                column: "MaxYearId",
                principalTable: "Years",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSubjectQualifications_Years_MinYearId",
                table: "TeacherSubjectQualifications",
                column: "MinYearId",
                principalTable: "Years",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_SchoolDays_SchoolDayId",
                table: "TimeSlots",
                column: "SchoolDayId",
                principalTable: "SchoolDays",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop re-added FKs
            migrationBuilder.DropForeignKey(
                name: "FK_CombinedLessonConfigs_Years_YearId",
                table: "CombinedLessonConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherDayConfigs_SchoolDays_SchoolDayId",
                table: "TeacherDayConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSubjectQualifications_Years_MaxYearId",
                table: "TeacherSubjectQualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSubjectQualifications_Years_MinYearId",
                table: "TeacherSubjectQualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_SchoolDays_SchoolDayId",
                table: "TimeSlots");

            // Reverse column/index renames in TimetableEntryClasses → TimetableEntryGroups
            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "TimetableEntryClasses",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_TimetableEntryClasses_ClassId",
                table: "TimetableEntryClasses",
                newName: "IX_TimetableEntryGroups_GroupId");

            // Reverse column/index renames in CombinedLessonClasses → CombinedLessonGroups
            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "CombinedLessonClasses",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_CombinedLessonClasses_ClassId",
                table: "CombinedLessonClasses",
                newName: "IX_CombinedLessonGroups_GroupId");

            // Reverse column/index renames in YearDayConfigs → GradeDayConfigs
            migrationBuilder.RenameColumn(
                name: "YearId",
                table: "YearDayConfigs",
                newName: "GradeId");

            migrationBuilder.RenameColumn(
                name: "SchoolDayId",
                table: "YearDayConfigs",
                newName: "TeachingDayId");

            migrationBuilder.RenameIndex(
                name: "IX_YearDayConfigs_YearId",
                table: "YearDayConfigs",
                newName: "IX_GradeDayConfigs_GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_YearDayConfigs_SchoolDayId",
                table: "YearDayConfigs",
                newName: "IX_GradeDayConfigs_TeachingDayId");

            migrationBuilder.RenameIndex(
                name: "IX_YearDayConfigs_TenantId_YearId_SchoolDayId",
                table: "YearDayConfigs",
                newName: "IX_GradeDayConfigs_TenantId_GradeId_TeachingDayId");

            // Reverse column/index renames in YearCurricula → GradeSubjectRequirements
            migrationBuilder.RenameColumn(
                name: "YearId",
                table: "YearCurricula",
                newName: "GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_YearCurricula_YearId",
                table: "YearCurricula",
                newName: "IX_GradeSubjectRequirements_GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_YearCurricula_SubjectId",
                table: "YearCurricula",
                newName: "IX_GradeSubjectRequirements_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_YearCurricula_TenantId_YearId_SubjectId",
                table: "YearCurricula",
                newName: "IX_GradeSubjectRequirements_TenantId_GradeId_SubjectId");

            // Reverse column/index renames in Classes → Groups
            migrationBuilder.RenameColumn(
                name: "YearId",
                table: "Classes",
                newName: "GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_YearId",
                table: "Classes",
                newName: "IX_Groups_GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_Classes_TenantId_YearId_Name",
                table: "Classes",
                newName: "IX_Groups_TenantId_GradeId_Name");

            // Reverse index renames in SchoolDays → TeachingDays
            migrationBuilder.RenameIndex(
                name: "IX_SchoolDays_TenantId_DayOfWeek",
                table: "SchoolDays",
                newName: "IX_TeachingDays_TenantId_DayOfWeek");

            // Reverse index renames in Years → Grades
            migrationBuilder.RenameIndex(
                name: "IX_Years_TenantId_Name",
                table: "Years",
                newName: "IX_Grades_TenantId_Name");

            // Reverse column/index renames in CombinedLessonConfigs
            migrationBuilder.RenameColumn(
                name: "MaxClassesPerLesson",
                table: "CombinedLessonConfigs",
                newName: "MaxGroupsPerLesson");

            migrationBuilder.RenameColumn(
                name: "YearId",
                table: "CombinedLessonConfigs",
                newName: "GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_CombinedLessonConfigs_YearId",
                table: "CombinedLessonConfigs",
                newName: "IX_CombinedLessonConfigs_GradeId");

            // Reverse column/index renames in TeacherDayConfigs
            migrationBuilder.RenameColumn(
                name: "SchoolDayId",
                table: "TeacherDayConfigs",
                newName: "TeachingDayId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherDayConfigs_SchoolDayId",
                table: "TeacherDayConfigs",
                newName: "IX_TeacherDayConfigs_TeachingDayId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherDayConfigs_TeacherId_SchoolDayId",
                table: "TeacherDayConfigs",
                newName: "IX_TeacherDayConfigs_TeacherId_TeachingDayId");

            // Reverse column/index renames in TeacherSubjectQualifications
            migrationBuilder.RenameColumn(
                name: "MinYearId",
                table: "TeacherSubjectQualifications",
                newName: "MinGradeId");

            migrationBuilder.RenameColumn(
                name: "MaxYearId",
                table: "TeacherSubjectQualifications",
                newName: "MaxGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherSubjectQualifications_MinYearId",
                table: "TeacherSubjectQualifications",
                newName: "IX_TeacherSubjectQualifications_MinGradeId");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherSubjectQualifications_MaxYearId",
                table: "TeacherSubjectQualifications",
                newName: "IX_TeacherSubjectQualifications_MaxGradeId");

            // Reverse column renames in TimeSlots
            migrationBuilder.RenameColumn(
                name: "SchoolDayId",
                table: "TimeSlots",
                newName: "TeachingDayId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeSlots_SchoolDayId_SlotNumber",
                table: "TimeSlots",
                newName: "IX_TimeSlots_TeachingDayId_SlotNumber");

            // Reverse column rename in TimetableEntries
            migrationBuilder.RenameColumn(
                name: "CombinedLessonClassId",
                table: "TimetableEntries",
                newName: "CombinedLessonGroupId");

            // Rename tables back
            migrationBuilder.RenameTable(
                name: "TimetableEntryClasses",
                newName: "TimetableEntryGroups");

            migrationBuilder.RenameTable(
                name: "CombinedLessonClasses",
                newName: "CombinedLessonGroups");

            migrationBuilder.RenameTable(
                name: "YearDayConfigs",
                newName: "GradeDayConfigs");

            migrationBuilder.RenameTable(
                name: "YearCurricula",
                newName: "GradeSubjectRequirements");

            migrationBuilder.RenameTable(
                name: "SchoolDays",
                newName: "TeachingDays");

            migrationBuilder.RenameTable(
                name: "Classes",
                newName: "Groups");

            migrationBuilder.RenameTable(
                name: "Years",
                newName: "Grades");

            // Re-add original FKs
            migrationBuilder.AddForeignKey(
                name: "FK_CombinedLessonConfigs_Grades_GradeId",
                table: "CombinedLessonConfigs",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherDayConfigs_TeachingDays_TeachingDayId",
                table: "TeacherDayConfigs",
                column: "TeachingDayId",
                principalTable: "TeachingDays",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSubjectQualifications_Grades_MaxGradeId",
                table: "TeacherSubjectQualifications",
                column: "MaxGradeId",
                principalTable: "Grades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSubjectQualifications_Grades_MinGradeId",
                table: "TeacherSubjectQualifications",
                column: "MinGradeId",
                principalTable: "Grades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_TeachingDays_TeachingDayId",
                table: "TimeSlots",
                column: "TeachingDayId",
                principalTable: "TeachingDays",
                principalColumn: "Id");
        }
    }
}
