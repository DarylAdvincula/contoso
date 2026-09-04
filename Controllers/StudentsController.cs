
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Models;
using ContosoUniversity.Data;
using ContosoUniversity;

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

        ViewData["LastSortOrder"] = sortOrder.StartsWith("last")
            ? sortOrder
            : ViewData["LastSortOrder"];

        ViewData["FirstSortOrder"] = sortOrder.StartsWith("first")
            ? sortOrder
            : ViewData["FirstSortOrder"];

        ViewData["DateSortOrder"] = sortOrder.StartsWith("date")
            ? sortOrder
            : ViewData["DateSortOrder"];

        // store the final sort order and search string
        ViewData["SortOrder"] = sortOrder;
        ViewData["SearchString"] = searchString ?? "";

        // build the initial query
        IQueryable<Student> students = _context.Students;

        if (!String.IsNullOrEmpty(searchString))
        {
            students = students.Where(s => 
                s.LastName.Contains(searchString) ||
                s.FirstMidName.Contains(searchString)
            );
        }

        switch (sortOrder)
        {
            case "date_oldest":
                students = students.OrderByDescending(s => s.EnrollmentDate);
                break;

            case "date_latest":
                students = students.OrderBy(s => s.EnrollmentDate);
                break;

            case "first_desc":
                students = students.OrderByDescending(s => s.FirstMidName);
                break;

            case "first_asc":
                students = students.OrderBy(s => s.FirstMidName);
                break;

            case "last_desc":
                students = students.OrderByDescending(s => s.LastName);
                break;

            default: // ascending name
                students = students.OrderBy(s => s.LastName);
                break;
        }

        return View(
            await Page<Student>.CreateAsync(
                query: students,
                pageNumber: page ?? 1,
                pageSize: 5
            )
        );
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var student = await _context.Students
            .Include(s => s.Enrollments)
                .ThenInclude(s => s.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        return View(student);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("EnrollmentDate,FirstMidName,LastName")]
        Student student
    )
    {
        try
        {
            if (ModelState.IsValid)
            {
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("",
                "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator."
            );
        }

        return View(student);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var student = await _context.Students.FindAsync(id);

        if (student == null)
            return NotFound();

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,
        [Bind("EnrollmentDate,LastName,FirstMidName")]
        Student student
    )
    {
        if (id == null)
            return NotFound();

        var studentToUpdate = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id);

        if (studentToUpdate == null)
            return NotFound();

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
            ModelState.AddModelError("",
                "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator."
            );
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "An unknown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator."
            );
        }

        return View(student);
    }

    public async Task<IActionResult> Delete(
        int? id,
        string? errorMessage
    )
    {
        if (id == null)
            return NotFound();

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

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        // no AsNoTracking and FirstOrDefaultAsync chain
        // because the target student's model should be tracked
        var student = await _context.Students.FindAsync(id);

        if (student == null)
            return RedirectToAction(nameof(Index));

        try
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            return RedirectToAction(nameof(Delete), new {
                id = id,
                errorMessage = "Delete failed. Try again, " + 
                    "and if the problem persists, " +
                    "see your system administrator."
            });
        }
        catch (Exception)
        {
            return RedirectToAction(nameof(Delete), new {
                id = id,
                errorMessage = "An unknown error occurred. Try again, " +
                    "and if the problem persists, " +
                    "see your system administrator."
            });
        }
    }
}