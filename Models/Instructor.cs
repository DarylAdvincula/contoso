using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class Instructor
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("FirstName")]
    [Display(Name = "First Name")]
    public string FirstMidName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; }

    [Display(Name = "Full Name")]
    public string FullName
    {
        get
        {
            return LastName + ", " + FirstMidName;
        }
    }

    public ICollection<CourseAssignment> CourseAssignments { get; set; } = [];
    public ICollection<Department> DepartmentsWhereAdmin { get; set; } = [];

    public OfficeAssignment? OfficeAssignment { get; set; }
}
