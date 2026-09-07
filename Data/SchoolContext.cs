using ContosoUniversity.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Data;

public class SchoolContext : DbContext
{
    public SchoolContext(DbContextOptions options)
        : base(options)
    {

    }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
    public DbSet<CourseAssignment> CourseAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // instruct ef core to name each entity table as specified
        modelBuilder.Entity<Course>().ToTable("Course");
        modelBuilder.Entity<Enrollment>().ToTable("Enrollment");
        modelBuilder.Entity<Department>().ToTable("Department");
        modelBuilder.Entity<OfficeAssignment>().ToTable("OfficeAssignment");
        modelBuilder.Entity<CourseAssignment>().ToTable("CourseAssignment");

        modelBuilder.Entity<Person>()
            .ToTable("Person")
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Instructor>("Instructor")
            .HasValue<Student>("Student");

        modelBuilder.Entity<CourseAssignment>()
            .HasKey(ca => new { ca.CourseId, ca.InstructorId });

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Administrator)
            .WithMany()
            .HasForeignKey(d => d.InstructorId);

        modelBuilder.Entity<Department>()
            .Property(p => p.RowVersion)
            .IsConcurrencyToken();
    }
}
