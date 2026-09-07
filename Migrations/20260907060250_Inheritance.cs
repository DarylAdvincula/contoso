using Humanizer;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoUniversity.Migrations
{
    /// <inheritdoc />
    public partial class Inheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // drop foreign keys and indexes pointing to tables we're going to drop.
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Student_StudentId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_StudentId",
                table: "Enrollment");

            // rename the Instructor table to Person
            migrationBuilder.RenameTable(
                name: "Instructor",
                newName: "Person");

            // add Student columns to the newly named Person table
            migrationBuilder.AddColumn<DateTime>(
                name: "EnrollmentDate",
                table: "Person",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Person",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "Instructor");

            // make HireDate nullable (since Students won't have a HireDate)
            migrationBuilder.AlterColumn<DateTime>(
                name: "HireDate",
                table: "Person",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // create a temporary column to securely map old primary keys
            migrationBuilder.AddColumn<int>(
                name: "OldId",
                table: "Person",
                type: "int",
                nullable: true);

            // copy existing Student data into new Person table.
            migrationBuilder.Sql(@"
                BEGIN
                    INSERT INTO Person (LastName, FirstName, HireDate, EnrollmentDate, Discriminator, OldId) 
                    SELECT LastName, FirstName, NULL AS HireDate, EnrollmentDate, 'Student' AS Discriminator, Id AS OldId FROM Student
                END
            ");

            // 7. Fix up existing relationships to match new PK's.
            migrationBuilder.Sql("UPDATE Enrollment SET StudentId = (SELECT Id FROM Person WHERE OldId = Enrollment.StudentId AND Discriminator = 'Student')");

            // 8. Clean up the temporary column and drop the obsolete Student table
            migrationBuilder.DropColumn(
                name: "OldId",
                table: "Person");

            migrationBuilder.DropTable(
                name: "Student");

            // 9. Re-wire foreign keys and indexes back onto the Person table
            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Person_StudentId",
                table: "Enrollment",
                column: "StudentId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_StudentId",
                table: "Enrollment",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Department_Instructor_InstructorId",
                table: "Department");

            migrationBuilder.DropColumn(
                name: "RowVersion",
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
