using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models;

public class Department
{
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    // sets the column's database type to money, it is
    // more appropriate since the column will hold currency amount
    [DataType(DataType.Currency)]
    [Column(TypeName = "money")]
    public decimal Budget { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    public int? InstructorId { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public Instructor? Administrator { get; set; }

    // courses that are being handled by the department
    public ICollection<Course> Courses { get; set; } = [];
}

