
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Models;
using ContosoUniversity.Data;

public class CoursesController : Controller
{
    private readonly SchoolContext _context;

    public CoursesController(SchoolContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()    
    {
        var courses = await _context.Courses
            .Include(c => c.Department)
            .AsNoTracking()
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> Details(int? id)
    {
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Title,Credits,DepartmentId")]
        Course course
    )
    {
        return View();
    }

    public async Task<IActionResult> Edit(int? id)
    {
        return View();
    }

    [HttpPost]
    [ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(
        int? id,
        
        [Bind("Id,Title,Credits,DepartmentId")]
        Course course
    )
    {
        return View();
    }

    public async Task<IActionResult> Delete(int? id)
    {
        return View();
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        return View();
    }
}
