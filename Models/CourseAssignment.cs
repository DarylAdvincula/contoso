namespace ContosoUniversity.Models;

public class CourseAssignment
{
    // pure junction table of instructors and courses
    public int InstructorId { get; set; }
    public int CourseId { get; set; }

    public Instructor Instructor { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
