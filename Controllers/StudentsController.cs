
using ContosoUniversity;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class StudentsController : Controller
{
    private readonly SchoolContext _context;

    public StudentsController(SchoolContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? sortOrder,
        string? searchString,
        int? page
    )
    {
        // supported sort values
        List<string> sortOrders = new List<string> {
            "last_asc",
            "last_desc",
            "first_asc",
            "first_desc",
            "date_latest",
            "date_oldest",
        };

        // if not provided, default to ascending name order
        sortOrder = sortOrder ?? "last_asc";

        // if provided but unrecognized, default to ascending name order
        // otherwise, set the provided sort order as the sort order to follow
        sortOrder = sortOrders.Contains(sortOrder.ToLower())
            ? sortOrder.ToLower()
            : "last_asc";

        if (sortOrder.StartsWith("last"))
            ViewData["LastSortOrder"] = sortOrder;

        if (sortOrder.StartsWith("first"))
            ViewData["FirstSortOrder"] = sortOrder;

        if (sortOrder.StartsWith("date"))
            ViewData["DateSortOrder"] = sortOrder;

        // store the final sort order and search string
        ViewData["SortOrder"] = sortOrder;
        ViewData["SearchString"] = searchString;

        // build the initial query
        IQueryable<Student> studentsQuery = _context.Students;

        if (!String.IsNullOrEmpty(searchString))
        {
            studentsQuery = studentsQuery.Where(s => 
                s.LastName.Contains(searchString) ||
                s.FirstMidName.Contains(searchString)
            );
        }

        switch (sortOrder)
        {
            case "date_oldest":
                studentsQuery = studentsQuery.OrderByDescending(s => s.EnrollmentDate);
                break;

            case "date_latest":
                studentsQuery = studentsQuery.OrderBy(s => s.EnrollmentDate);
                break;

            case "first_desc":
                studentsQuery = studentsQuery.OrderByDescending(s => s.FirstMidName);
                break;

            case "first_asc":
                studentsQuery = studentsQuery.OrderBy(s => s.FirstMidName);
                break;

            case "last_desc":
                studentsQuery = studentsQuery.OrderByDescending(s => s.LastName);
                break;

            default: // ascending last name
                studentsQuery = studentsQuery.OrderBy(s => s.LastName);
                break;
        }

        return View(
            await Page<Student>.CreateAsync(
                query: studentsQuery,
                pageNumber: page ?? 1
            )
        );
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return BadRequest();

        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(s => s.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        return View(student);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("EnrollmentDate,FirstMidName,LastName")]
        Student student,

        string[] selectedCourses
    )
    {
        if (!ModelState.IsValid)
            return View(student);

        string errorMessage = string.Empty;

        try
        {
            _context.Students.Add(student);
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
        return View(student);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

        var student = await _context.Students
            .Include(s => s.Enrollments)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,

        [Bind("EnrollmentDate,LastName,FirstMidName")]
        Student student,

        string[] selectedCourses
    )
    {
        if (id == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(student);

        var studentToUpdate = await _context.Students
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (studentToUpdate == null)
            return NotFound();

        string errorMessage = string.Empty;

        try
        {
            studentToUpdate.EnrollmentDate = student.EnrollmentDate;
            studentToUpdate.FirstMidName = student.FirstMidName;
            studentToUpdate.LastName = student.LastName;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }
        catch (Exception ex)
        {
            errorMessage = "An unknown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator. " + ex.Message;
        }

        ModelState.AddModelError("", errorMessage);
        return View(student);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(
        int? id,
        string? errorMessage
    )
    {
        if (id == null)
            return BadRequest();

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (student == null)
            return NotFound();

        // check if an error occurred during deletion and
        // add a view data
        if (!String.IsNullOrEmpty(errorMessage))
            ViewData["ErrorMessage"] = errorMessage;

        return View(student);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return BadRequest();

        // no AsNoTracking and FirstOrDefaultAsync chain
        // because the target student's model should be tracked
        var student = await _context.Students.FindAsync(id);

        if (student == null)
            return RedirectToAction(nameof(Index));

        string errorMessage = string.Empty;

        try
        {
            _context.Students.Remove(student);
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
}