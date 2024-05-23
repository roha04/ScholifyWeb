using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScholifyWeb.Migrations
{
    /// <inheritdoc />
    public partial class FKTeacherClassId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Users_TeacherId",
                table: "Classes");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "Classes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Users_TeacherId",
                table: "Classes",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Users_TeacherId",
                table: "Classes");

            migrationBuilder.AlterColumn<int>(
                name: "TeacherId",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Users_TeacherId",
                table: "Classes",
                column: "TeacherId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
