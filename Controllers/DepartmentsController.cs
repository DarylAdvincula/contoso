
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            return BadRequest();

        //var department = await _context.Departments
        //    .Include(d => d.Administrator)
        //    .AsNoTracking()
        //    .FirstOrDefaultAsync(m => m.Id == id);

        string query = "SELECT * FROM Department WHERE Id = @p0";
        var department = await _context.Departments
            .FromSqlRaw(query, id)
            .Include(d => d.Administrator)
            .AsNoTracking()
            .SingleOrDefaultAsync();

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
        if (!ModelState.IsValid)
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

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();

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

        [Bind("Name,Budget,StartDate,InstructorId,RowVersion")] 
        Department department
    )
    {
        if (id == null)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            PopulateAdministratorsDropDownList(department.InstructorId);
            return View(department);
        }

        var departmentToUpdate = await _context.Departments
            .FindAsync (id);

        if (departmentToUpdate == null)
        {
            var deletedDepartment = new Department();

            ModelState.AddModelError("",
                "Unable to save changes. " +
                "The department was deleted by another user."
            );
            PopulateAdministratorsDropDownList(department.InstructorId);
            return View(deletedDepartment);
        }

        string errorMessage = string.Empty;

        try
        {
            _context.Entry(departmentToUpdate).Property("RowVersion").OriginalValue = department.RowVersion;

            departmentToUpdate.Name = department.Name;
            departmentToUpdate.Budget = department.Budget;
            departmentToUpdate.StartDate = department.StartDate;
            departmentToUpdate.InstructorId = department.InstructorId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var clientValues = (Department) entry.Entity;
            var databaseEntry = entry.GetDatabaseValues();

            if (databaseEntry == null)
            {
                errorMessage = "Unable to save changes. " +
                    "The department was deleted by another user.";
            }
            else
            {
                var databaseValues = (Department) databaseEntry.ToObject();

                if (databaseValues.Name != clientValues.Name)
                {
                    ModelState.AddModelError(
                        "Name", 
                        "Current value: " + databaseValues.Name
                    );
                }
                if (databaseValues.Budget != clientValues.Budget)
                {
                    ModelState.AddModelError(
                        "Budget", 
                        "Current value: " + String.Format("{0:c}", databaseValues.Budget)
                    );
                }
                if (databaseValues.StartDate != clientValues.StartDate)
                {
                    ModelState.AddModelError(
                        "StartDate", 
                        "Current value: " + String.Format("{0:d}", databaseValues.StartDate)
                    );
                }
                if (databaseValues.InstructorId != clientValues.InstructorId)
                {
                    ModelState.AddModelError(
                        "InstructorId",
                        "Current value: " + _context.Instructors.Find(databaseValues.InstructorId)?.FullName
                    );
                }
                if (databaseValues.StartDate != clientValues.StartDate)
                {
                    ModelState.AddModelError(
                        "StartDate",
                        "Current value: " + String.Format("{0:d}", databaseValues.StartDate)
                    );
                }

                ModelState.AddModelError("",
                    "The record you attempted to edit " +
                    "was modified by another user after you got the original value. The " +
                    "edit operation was canceled and the current values in the database " +
                    "have been displayed. If you still want to edit this record, click " +
                    "the Save button again. Otherwise click the Back to List hyperlink."
                );

                // apply the current RowVersion
                department.RowVersion = databaseValues.RowVersion;

                // remove the previous RowVersion validation status 
                // prevents having the same model state error and lets
                // the user to save their changes
                ModelState.Remove(nameof(Department.RowVersion));
            }
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

    [HttpGet]
    public async Task<IActionResult> Delete(
        int? id,
        string? errorMessage
    )
    {
        if (id == null)
            return BadRequest();

        var department = await _context.Departments
            .Include(d => d.Administrator)
            .FirstOrDefaultAsync(m => m.Id == id);

        bool hasError = !String.IsNullOrEmpty(errorMessage);

        if (hasError)
            ViewData["ErrorMessage"] = errorMessage;

        if (department == null)
        {
            if (hasError)
                return RedirectToAction(nameof(Index));

            return NotFound();
        }

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Department department)
    {
        var departmentToDelete = await _context.Departments
        .FindAsync(department.Id);

        // department deltails was already loaded at the delete ui but was deleted
        // first by another user
        if (departmentToDelete == null)
        {
            ViewBag.DeletedAlready = true;
            ViewData["ErrorMessage"] = "This department has already been deleted by another user.";
            return View(department);
        }

        try
        {
            _context.Entry(departmentToDelete).Property("RowVersion").OriginalValue = department.RowVersion;
            _context.Entry(departmentToDelete).State = EntityState.Deleted;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            var databaseValues = await _context.Departments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == department.Id);

            if (databaseValues == null)
            {
                // the department still exist during the execution of this method on every user that attempts
                // to delete it but others might have deleted it splitsecond earlier
                ViewBag.DeletedAlready = true;
                ViewData["ErrorMessage"] = "This department was just deleted by another user in another session.";
                return View(department);
            }
            else
            {
                // someone modified it before the current user deletes it causing the row version to change
                // and cancel the current user's delete operation
                return RedirectToAction(nameof(Delete), new
                {
                    id = department.Id,
                    errorMessage = "The record you are trying to delete was modified by another user " +
                        "after you loaded the page. The delete operation was canceled so you " +
                        "can review the current database values displayed below. If you still " +
                        "want to delete this department, click Delete again."
                });
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError("", 
                "Unable to delete. Try again, " + 
                "and if the problem persists, " + 
                "contact your system administrator."
            );
            return View(department);
        }
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
