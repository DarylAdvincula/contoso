namespace ContosoUniversity.Models.SchoolViewModels;

public class EnrolledCourseData
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Enrolled { get; set; }
}