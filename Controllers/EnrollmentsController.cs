
using ContosoUniversity;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class EnrollmentsController : Controller
{
    private readonly SchoolContext _context;

    public EnrollmentsController(SchoolContext context)
    {
        _context = context;
    }

    // GET: ENROLLMENTS
    public async Task<IActionResult> Index(
        int? page,
        int? SelectedCourse,
        string? SearchString
    )
    {
        ViewData["SearchString"] = SearchString;

        var courses = await _context.Courses
            .OrderBy(q => q.Title)
            .ToListAsync();

        ViewBag.SelectedCourse = new SelectList(
            courses,
            "Id",
            "Title",
            SelectedCourse
        );

        var courseId = SelectedCourse.GetValueOrDefault();

        IQueryable<Enrollment> enrollmentsQuery = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => 
                !SelectedCourse.HasValue || 
                e.Course!.Id == courseId
            );

        if (!String.IsNullOrEmpty(SearchString))
        {
            enrollmentsQuery = enrollmentsQuery
                .Where(e => 
                    e.Student!.LastName.Contains(SearchString) ||
                    e.Student!.FirstMidName.Contains(SearchString)
                );
        }

        enrollmentsQuery = enrollmentsQuery
            .OrderBy(e => e.Student!.LastName);

        return View(
            await Page<Enrollment>.CreateAsync(
                query: enrollmentsQuery,
                pageNumber: page ?? 1
            )
        );
    }

    // GET: ENROLLMENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return BadRequest();

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        return View(enrollment);
    }

    [HttpGet]
    public IActionResult Create()
    {
        PopulateCoursesDropDownList();
        PopulateStudentDropDownList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourseId,StudentId,Grade")] Enrollment enrollment)
    {
        if (ModelState.IsValid)
        {
            _context.Add(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(enrollment);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        PopulateCoursesDropDownList(enrollment.CourseId);
        PopulateStudentDropDownList(enrollment.StudentId);
        return View(enrollment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id, 
        
        [Bind("CourseId,StudentId,Grade")] 
        Enrollment enrollment
    )
    {
        if (id == null)
            return BadRequest();

        var enrollmentToUpdate = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollmentToUpdate == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            PopulateCoursesDropDownList(enrollment.CourseId);
            PopulateStudentDropDownList(enrollment.StudentId);
            return View(enrollment);
        }

        string errorMessage = string.Empty;

        try
        {
            enrollmentToUpdate.StudentId = enrollment.StudentId;
            enrollmentToUpdate.CourseId = enrollment.CourseId;
            enrollmentToUpdate.Grade = enrollment.Grade;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }
        catch (Exception)
        {
            errorMessage = "An unknown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }

        ModelState.AddModelError("", errorMessage);
        PopulateCoursesDropDownList(enrollment.CourseId);
        PopulateStudentDropDownList(enrollment.StudentId);
        return View(enrollment);
    }

    private void PopulateCoursesDropDownList(object? selectedCourse = null)
    {
        var coursesQuery = _context.Courses
            .OrderBy(c => c.Title);

        ViewBag.CourseId = new SelectList(
            items: coursesQuery.AsNoTracking(),
            dataValueField: "Id",
            dataTextField: "Title",
            selectedValue: selectedCourse
        );
    }

    private void PopulateStudentDropDownList(object? selectedStudent = null)
    {
        var studentsQuery = _context.Students
            .OrderBy(c => c.LastName);

        ViewBag.StudentId = new SelectList(
            items: studentsQuery.AsNoTracking(),
            dataValueField: "Id",
            dataTextField: "FullName",
            selectedValue: selectedStudent
        );
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id, string? errorMessage)
    {
        if (id == null)
            return BadRequest();

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound();

        if (!String.IsNullOrEmpty(errorMessage))
            ViewData["ErrorMessage"] = errorMessage;

        return View(enrollment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return BadRequest();

        var enrollment = await _context.Enrollments
            .FindAsync(id);

        if (enrollment == null)
            return NotFound();

        string errorMessage = string.Empty;

        try
        {
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Delete failed. Try again, " +
                "and if the problem persists, " +
                "see your system administrator.";
        }
        catch (Exception)
        {
            errorMessage = "An unknown error occurred. Try again, " +
                "and if the problem persists, " +
                "see your system administrator.";
        }

        return RedirectToAction(
            nameof(Delete),
            new { id, errorMessage }
        );
    }

    private bool EnrollmentExists(int? id)
    {
        return _context.Enrollments.Any(e => e.Id == id);
    }
}
