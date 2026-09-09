
using ContosoUniversity;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class CoursesController : Controller
{
    private readonly SchoolContext _context;

    public CoursesController(SchoolContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> Index(int? SelectedDepartment, int? page)
    {
        var departments = await _context.Departments
            .OrderBy(q => q.Name)
            .ToListAsync();

        ViewBag.SelectedDepartment = new SelectList(
            departments, 
            "Id", 
            "Name", 
            SelectedDepartment
        );

        int departmentID = SelectedDepartment.GetValueOrDefault();

        IQueryable<Course> courses = _context.Courses
            .Where(c => !SelectedDepartment.HasValue || c.DepartmentId == departmentID)
            .OrderBy(c => c.Id)
            .Include(d => d.Department);

        return View(
            await Page<Course>.CreateAsync(
                query: courses, 
                pageNumber: page ?? 1
            )
        );
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return BadRequest();

        var course = await _context.Courses
            .Include(c => c.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound();

        return View(course);
    }

    [HttpGet]
    public IActionResult Create()
    {
        PopulateDepartmentsDropDownList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Credits,DepartmentId,Title")]
        Course course
    )
    {
        if (!ModelState.IsValid)
        {
            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        string errorMessage = string.Empty;

        try
        {
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator. ";
        }
        catch (Exception)
        {
            errorMessage = "An unknown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";

        }

        ModelState.AddModelError("", errorMessage);
        PopulateDepartmentsDropDownList(course.DepartmentId);
        return View(course);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound();

        PopulateDepartmentsDropDownList(course.DepartmentId);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id,

        [Bind("Title,Credits,DepartmentId")]
        Course course
    )
    {
        if (id == null)
            return BadRequest();

        var courseToUpdate = await _context.Courses
            .FindAsync(id);

        if (courseToUpdate == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentId);
            return View(course);
        }

        string errorMessage = string.Empty;

        try
        {
            courseToUpdate.Title = course.Title;
            courseToUpdate.Credits = course.Credits;
            courseToUpdate.DepartmentId = course.DepartmentId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            errorMessage = "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }
        catch
        {
            errorMessage = "An unknown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }

        ModelState.AddModelError("", errorMessage);
        PopulateDepartmentsDropDownList(courseToUpdate.DepartmentId);
        return View(courseToUpdate);
    }

    public ActionResult UpdateCourseCredits()
    {
        return View();
    }

    [HttpPost]
    public ActionResult UpdateCourseCredits(int? multiplier)
    {
        if (multiplier != null)
        {
            FormattableString query = $"UPDATE Course SET Credits = Credits * {multiplier}";

            ViewBag.RowsAffected = _context.Database
                .ExecuteSql(query);
        }

        return View();
    }


    [HttpGet]
    public async Task<IActionResult> Delete(
        int? id, 
        string? errorMessage
    )
    {
        if (id == null)
            return BadRequest();

        var course = await _context.Courses
            .Include(c => c.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound();

        if (!String.IsNullOrEmpty(errorMessage))
            ViewData["ErrorMessage"] = errorMessage;

        return View(course);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
            return BadRequest();

        var course = await _context.Courses
            .FindAsync(id);

        if (course == null)
            return NotFound();

        string errorMessage = string.Empty;

        try
        {
            _context.Courses.Remove(course);
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

    private void PopulateDepartmentsDropDownList(object? selectedDepartment = null)
    {
        var departmentsQuery = _context.Departments
            .OrderBy(d => d.Name);

        // the view's blueprint for creating a select tag that's named DepartmentId
        ViewBag.DepartmentId = new SelectList(
            items: departmentsQuery.AsNoTracking(),
            dataValueField: "Id",
            dataTextField: "Name",
            selectedValue: selectedDepartment
        );
    }
}
