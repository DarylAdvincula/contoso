namespace ContosoUniversity.Models.SchoolViewModels;

public class AssignedCourseData
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Assigned { get; set; }
}