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

        // set unique constraints
        modelBuilder.Entity<Course>()
            .HasIndex(c => c.Title)
            .IsUnique();

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        modelBuilder.Entity<Instructor>()
            .HasIndex(i => new { i.FirstMidName, i.LastName })
            .IsUnique();

        modelBuilder.Entity<Department>()
            .HasIndex(d => d.Name)
            .IsUnique();
        
        // model inheritance
        modelBuilder.Entity<Person>()
            .ToTable("Person")
            .HasDiscriminator<string>("Discriminator")
            .HasValue<Instructor>("Instructor")
            .HasValue<Student>("Student");

        // composite primary key for couse assignment (many course to many instructor)
        modelBuilder.Entity<CourseAssignment>()
            .HasKey(ca => new { ca.CourseId, ca.InstructorId });

        // specify which is the foreign key that will point to the administrator
        modelBuilder.Entity<Department>()
            .HasOne(d => d.Administrator)
            .WithMany()
            .HasForeignKey(d => d.InstructorId);

        // set RowVersion column as the concurrency key
        modelBuilder.Entity<Department>()
            .Property(p => p.RowVersion)
            .IsConcurrencyToken();
    }
}
