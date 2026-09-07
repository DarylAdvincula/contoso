
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Models;
using ContosoUniversity.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

public class DepartmentsController : Controller
{
    private readonly SchoolContext _context;

    public DepartmentsController(SchoolContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()    
    {
        var departments = await _context.Departments
            .Include(d => d.Administrator)
            .AsNoTracking()
            .ToListAsync();

        return View(departments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var department = await _context.Departments
            .Include(d => d.Administrator)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
            return NotFound();

        return View(department);
    }

    public IActionResult Create()
    {
        PopulateAdministratorsDropDownList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name,Budget,StartDate,InstructorId")] 
        Department department
    )
    {
        if (ModelState.IsValid)
        {
            PopulateAdministratorsDropDownList(department.InstructorId);
            return View(department);
        }

        string errorMessage = string.Empty;

        try
        {
            _context.Add(department);
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
            errorMessage = "An unkown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }

        ModelState.AddModelError("", errorMessage);
        PopulateAdministratorsDropDownList(department.InstructorId);
        return View(department);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var department = await _context.Departments
            .FindAsync(id);

        if (department == null)
            return NotFound();

        PopulateAdministratorsDropDownList(department.InstructorId);
        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? id, 

        [Bind("Name,Budget,StartDate,InstructorId")] 
        Department department
    )
    {
        if (id == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            PopulateAdministratorsDropDownList(department.InstructorId);
            return View(department);
        }

        var departmentToUpdate = await _context.Departments
            .FindAsync (id);

        if (departmentToUpdate == null)
            return NotFound();

        string errorMessage = string.Empty;

        try
        {
            departmentToUpdate.Name = department.Name;
            departmentToUpdate.Budget = department.Budget;
            departmentToUpdate.StartDate = department.StartDate;
            departmentToUpdate.InstructorId = department.InstructorId;

            _context.Update(department);
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
            errorMessage = "An unkown error occurred. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.";
        }

        ModelState.AddModelError("", errorMessage);
        PopulateAdministratorsDropDownList(department.InstructorId);
        return View(department);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var department = await _context.Departments
            .Include(d => d.Administrator)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
            return NotFound();

        return View(department);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department != null)
            _context.Departments.Remove(department);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void PopulateAdministratorsDropDownList(object? selectedAdministrator = null)
    {
        var instructorsQuery = _context.Instructors
            .OrderBy(i => i.LastName);

        ViewBag.AdministratorId = new SelectList(
            items: instructorsQuery.AsNoTracking(),
            dataValueField: "Id",
            dataTextField: "LastName",
            selectedValue: selectedAdministrator
        );
    }
}
