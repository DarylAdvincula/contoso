using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class Course
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(50, ErrorMessage = "Title must not exceed 50 characters.")]
    public string Title { get; set; } = string.Empty;

    public int Credits { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
