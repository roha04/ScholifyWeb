using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholifyWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixGradeRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Gradebooks_GradebookId",
                table: "Grades");

            migrationBuilder.AlterColumn<int>(
                name: "GradebookId",
                table: "Grades",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Gradebooks_GradebookId",
                table: "Grades",
                column: "GradebookId",
                principalTable: "Gradebooks",
                principalColumn: "GradebookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Gradebooks_GradebookId",
                table: "Grades");

            migrationBuilder.AlterColumn<int>(
                name: "GradebookId",
                table: "Grades",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Gradebooks_GradebookId",
                table: "Grades",
                column: "GradebookId",
                principalTable: "Gradebooks",
                principalColumn: "GradebookId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
