using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoUniversity.Migrations
{
    /// <inheritdoc />
    public partial class UpdatesFromUpdateRelatedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Instructor_InstructorID",
                table: "Department");

            migrationBuilder.RenameColumn(
                name: "InstructorID",
                table: "Department",
                newName: "InstructorId");

            migrationBuilder.RenameIndex(
                name: "IX_Department_InstructorID",
                table: "Department",
                newName: "IX_Department_InstructorId");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "OfficeAssignment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Instructor_InstructorId",
                table: "Department",
                column: "InstructorId",
                principalTable: "Instructor",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Instructor_InstructorId",
                table: "Department");

            migrationBuilder.RenameColumn(
                name: "InstructorId",
                table: "Department",
                newName: "InstructorID");

            migrationBuilder.RenameIndex(
                name: "IX_Department_InstructorId",
                table: "Department",
                newName: "IX_Department_InstructorID");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "OfficeAssignment",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Instructor_InstructorID",
                table: "Department",
                column: "InstructorID",
                principalTable: "Instructor",
                principalColumn: "Id");
        }
    }
}
