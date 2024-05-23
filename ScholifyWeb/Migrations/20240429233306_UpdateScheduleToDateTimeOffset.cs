using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholifyWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateScheduleToDateTimeOffset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Classes_ClassId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ClassId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Grades",
                newName: "Score");

            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "Grades",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_ScheduleId",
                table: "Grades",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Schedules_ScheduleId",
                table: "Grades",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "ScheduleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Schedules_ScheduleId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_ScheduleId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "Grades");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Grades",
                newName: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ClassId",
                table: "Schedules",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Classes_ClassId",
                table: "Schedules",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
