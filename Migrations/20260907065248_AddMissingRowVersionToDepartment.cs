using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoUniversity.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingRowVersionToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
               name: "RowVersion",
               table: "Department",
               type: "rowversion",
               rowVersion: true,
               nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseAssignment_Person_InstructorId",
                table: "CourseAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_Department_Person_InstructorId",
                table: "Department");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Person_StudentId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeAssignment_Person_InstructorId",
                table: "OfficeAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Person",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Person");

            migrationBuilder.RenameTable(
                name: "Person",
                newName: "Student");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EnrollmentDate",
                table: "Student",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Student",
                table: "Student",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Instructor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructor", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAssignment_Instructor_InstructorId",
                table: "CourseAssignment",
                column: "InstructorId",
                principalTable: "Instructor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Department_Instructor_InstructorId",
                table: "Department",
                column: "InstructorId",
                principalTable: "Instructor",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Student_StudentId",
                table: "Enrollment",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeAssignment_Instructor_InstructorId",
                table: "OfficeAssignment",
                column: "InstructorId",
                principalTable: "Instructor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
